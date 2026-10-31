using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.TunnelEngine;
using NetTunnel.Service.TunnelEngine.Http2;
using Xunit.Abstractions;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// Measures the per-byte overhead the parsers add to a connection. The numbers are written to the test output; the assertions are
    /// deliberately loose floors that only catch an order-of-magnitude regression (e.g. a return to per-byte allocation).
    /// </summary>
    public class PerformanceTests(ITestOutputHelper output)
    {
        private const int ReceiveSize = 64 * 1024;

        private static EndpointConfiguration Config(NtHttpHeaderType type)
        {
            var config = new EndpointConfiguration
            {
                Direction = type == NtHttpHeaderType.Request ? NtDirection.Inbound : NtDirection.Outbound,
                InboundTrafficType = NtTrafficType.Https,
                OutboundTrafficType = NtTrafficType.Https
            };
            config.HttpHeaderRules.Add(new HttpHeaderRule(NtHttpVerb.Any, "X-Forwarded-For", NtHttpHeaderAction.Upsert, "10.0.0.1")
            { HeaderType = type, Enabled = true });
            return config;
        }

        /// <summary>Feeds the stream repeatedly, in receive-buffer sized pieces, and returns MB/s.</summary>
        private double Measure(string label, Func<IHttpStreamProcessor> create, byte[] stream, int repeats, bool alloc = false)
        {
            var parser = create();
            var buffer = new byte[ReceiveSize];

            //Warm up.
            Feed(parser, stream, buffer);

            GC.Collect();
            long allocBefore = GC.GetAllocatedBytesForCurrentThread();
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < repeats; i++)
            {
                Feed(parser, stream, buffer);
            }
            timer.Stop();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocBefore;

            double total = (double)stream.Length * repeats;
            double mbps = total / (1024 * 1024) / timer.Elapsed.TotalSeconds;
            output.WriteLine($"{label,-34} {mbps,9:n0} MB/s   {allocated / total,6:n3} bytes allocated per byte");
            return mbps;
        }

        private static void Feed(IHttpStreamProcessor parser, byte[] stream, byte[] buffer)
        {
            for (int position = 0; position < stream.Length; position += ReceiveSize)
            {
                int size = Math.Min(ReceiveSize, stream.Length - position);
                Array.Copy(stream, position, buffer, 0, size);
                parser.Process(buffer, size);
            }
        }

        private static byte[] Http1Request(int bodyBytes, bool chunked)
        {
            var stream = new MemoryStream();
            var header = chunked
                ? "POST /upload HTTP/1.1\r\nHost: x\r\nTransfer-Encoding: chunked\r\n\r\n"
                : $"POST /upload HTTP/1.1\r\nHost: x\r\nContent-Length: {bodyBytes}\r\n\r\n";
            stream.Write(Encoding.ASCII.GetBytes(header));

            if (chunked)
            {
                for (int sent = 0; sent < bodyBytes; sent += 8192)
                {
                    stream.Write(Encoding.ASCII.GetBytes("2000\r\n"));
                    stream.Write(new byte[8192]);
                    stream.Write("\r\n"u8);
                }
                stream.Write("0\r\n\r\n"u8);
            }
            else
            {
                stream.Write(new byte[bodyBytes]);
            }
            return stream.ToArray();
        }

        private static byte[] Frame(byte type, byte flags, uint streamId, byte[] payload)
        {
            var frame = new byte[9 + payload.Length];
            frame[0] = (byte)(payload.Length >> 16);
            frame[1] = (byte)(payload.Length >> 8);
            frame[2] = (byte)payload.Length;
            frame[3] = type;
            frame[4] = flags;
            BinaryPrimitives.WriteUInt32BigEndian(frame.AsSpan(5), streamId);
            payload.CopyTo(frame, 9);
            return frame;
        }

        [Fact]
        public void Http1_LargeBodies_Throughput()
        {
            var mbps = Measure("HTTP/1 content-length body", () => new HttpStreamParser(Config(NtHttpHeaderType.Request)), Http1Request(32 * 1024 * 1024, false), 3);
            Assert.True(mbps > 200, $"content-length body throughput was {mbps:n0} MB/s");
        }

        [Fact]
        public void Http1_ChunkedBodies_Throughput()
        {
            var mbps = Measure("HTTP/1 chunked body (8KB chunks)", () => new HttpStreamParser(Config(NtHttpHeaderType.Request)), Http1Request(32 * 1024 * 1024, true), 3);
            Assert.True(mbps > 100, $"chunked body throughput was {mbps:n0} MB/s");
        }

        [Fact]
        public void Http1_SmallRequests_Throughput()
        {
            //Header-dominated traffic: many small keep-alive requests, each needing a rule applied.
            var one = Encoding.ASCII.GetBytes("GET /index.html HTTP/1.1\r\nHost: example.com\r\nUser-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64)\r\nAccept: text/html,application/xhtml+xml\r\nAccept-Encoding: gzip, deflate, br\r\nCookie: session=0123456789abcdef0123456789abcdef\r\n\r\n");
            var stream = Enumerable.Repeat(one, 20_000).SelectMany(o => o).ToArray();

            var parser = new HttpStreamParser(Config(NtHttpHeaderType.Request));
            var buffer = new byte[ReceiveSize];
            Feed(parser, stream, buffer);

            var timer = Stopwatch.StartNew();
            const int repeats = 5;
            for (int i = 0; i < repeats; i++)
            {
                Feed(parser, stream, buffer);
            }
            timer.Stop();

            double perSecond = 20_000.0 * repeats / timer.Elapsed.TotalSeconds;
            output.WriteLine($"{"HTTP/1 small requests",-34} {perSecond,9:n0} requests/s ({stream.Length * repeats / 1048576.0 / timer.Elapsed.TotalSeconds:n0} MB/s)");
            Assert.True(perSecond > 20_000, $"{perSecond:n0} requests/s");
        }

        [Fact]
        public void Http2_DataFrames_Throughput()
        {
            var stream = new MemoryStream();
            stream.Write("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8);
            var payload = new byte[16384];
            for (int i = 0; i < 2048; i++) //32 MB of DATA.
            {
                stream.Write(Frame(0, 0, 1, payload));
            }
            var bytes = stream.ToArray();

            var mbps = Measure("HTTP/2 DATA frames (16KB)", () =>
            {
                var parser = new Http2StreamParser(Config(NtHttpHeaderType.Request), true);
                return parser;
            }, bytes, 3);
            Assert.True(mbps > 200, $"DATA throughput was {mbps:n0} MB/s");
        }

        [Fact]
        public void Http2_Requests_Throughput()
        {
            byte[] block = [0x82, 0x86, 0x84, 0x41, 0x8c, 0xf1, 0xe3, 0xc2, 0xe5, 0xf2, 0x3a, 0x6b, 0xa0, 0xab, 0x90, 0xf4, 0xff];
            var stream = new MemoryStream();
            stream.Write("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8);
            uint id = 1;
            for (int i = 0; i < 20_000; i++, id += 2)
            {
                stream.Write(Frame(1, 5, id, block));
            }
            var bytes = stream.ToArray();

            var parser = new Http2StreamParser(Config(NtHttpHeaderType.Request), true);
            var buffer = new byte[ReceiveSize];
            Feed(parser, bytes, buffer);

            //The same parser continues: the preface is only valid once, so measure with fresh parsers.
            var timer = Stopwatch.StartNew();
            const int repeats = 5;
            for (int i = 0; i < repeats; i++)
            {
                Feed(new Http2StreamParser(Config(NtHttpHeaderType.Request), true), bytes, buffer);
            }
            timer.Stop();

            double perSecond = 20_000.0 * repeats / timer.Elapsed.TotalSeconds;
            output.WriteLine($"{"HTTP/2 requests",-34} {perSecond,9:n0} requests/s");
            Assert.True(perSecond > 20_000, $"{perSecond:n0} requests/s");
        }
    }
}

namespace NetTunnel.Tests
{
    public class LargeRequestPerformanceTests(ITestOutputHelper output)
    {
        private static EndpointConfiguration Config()
        {
            var config = new EndpointConfiguration { Direction = NtDirection.Inbound, InboundTrafficType = NtTrafficType.Http };
            config.HttpHeaderRules.Add(new HttpHeaderRule(NtHttpVerb.Any, "X-Forwarded-For", NtHttpHeaderAction.Upsert, "10.0.0.1")
            { HeaderType = NtHttpHeaderType.Request, Enabled = true });
            return config;
        }

        private double Run(string label, byte[] stream, int receiveSize, int repeats = 3)
        {
            double Pass()
            {
                var parser = new HttpStreamParser(Config());
                var buffer = new byte[receiveSize];
                var timer = Stopwatch.StartNew();
                for (int position = 0; position < stream.Length; position += receiveSize)
                {
                    int size = Math.Min(receiveSize, stream.Length - position);
                    Array.Copy(stream, position, buffer, 0, size);
                    parser.Process(buffer, size);
                }
                return stream.Length / 1048576.0 / timer.Elapsed.TotalSeconds;
            }

            Pass();
            double best = 0;
            for (int i = 0; i < repeats; i++) best = Math.Max(best, Pass());
            output.WriteLine($"{label,-52} {best,9:n0} MB/s");
            return best;
        }

        private static byte[] Chunked(int total, int chunkSize)
        {
            var s = new MemoryStream();
            s.Write(Encoding.ASCII.GetBytes("POST /u HTTP/1.1\r\nHost: x\r\nTransfer-Encoding: chunked\r\n\r\n"));
            var chunk = new byte[chunkSize];
            var prefix = Encoding.ASCII.GetBytes(chunkSize.ToString("x") + "\r\n");
            for (int sent = 0; sent < total; sent += chunkSize) { s.Write(prefix); s.Write(chunk); s.Write("\r\n"u8); }
            s.Write("0\r\n\r\n"u8);
            return s.ToArray();
        }

        private static byte[] WithLength(int total, string extraHeaders = "")
        {
            var s = new MemoryStream();
            s.Write(Encoding.ASCII.GetBytes($"POST /u HTTP/1.1\r\nHost: x\r\n{extraHeaders}Content-Length: {total}\r\n\r\n"));
            s.Write(new byte[total]);
            return s.ToArray();
        }

        [Fact]
        public void LargeRequests()
        {
            const int total = 64 * 1024 * 1024;
            Run("content-length 64MB, 16KB receives", WithLength(total), 16 * 1024);
            Run("content-length 64MB, 1460-byte receives", WithLength(total), 1460);
            Run("chunked 64MB, 64KB chunks, 16KB receives", Chunked(total, 64 * 1024), 16 * 1024);
            Run("chunked 64MB, 8KB chunks, 16KB receives", Chunked(total, 8 * 1024), 16 * 1024);
            Run("chunked 64MB, 1KB chunks, 16KB receives", Chunked(total, 1024), 16 * 1024);
            Run("chunked 16MB, 100-byte chunks, 16KB receives", Chunked(16 * 1024 * 1024, 100), 16 * 1024);

            var bigCookie = $"Cookie: {new string('c', 16000)}\r\n";
            Run("16KB header, then 1MB body, 16KB receives", WithLength(1024 * 1024, bigCookie), 16 * 1024, 20);
            Run("16KB header, 1460-byte receives (split header)", WithLength(1024, bigCookie), 1460, 2000);
        }
    }
}
