using System.Net.Sockets;
using System.Text;
using NetTunnel.Library.Payloads;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// Real sockets through a real inbound endpoint and a real outbound endpoint, joined by a tunnel that delivers its notifications
    /// concurrently and out of order the way the messaging layer does (see <see cref="LoopbackTunnelSide"/>).
    /// </summary>
    [Collection("ServiceState")]
    public class TunnelDataPathTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private static byte[] RandomBytes(int length, int seed)
        {
            var bytes = new byte[length];
            new Random(seed).NextBytes(bytes);
            return bytes;
        }

        private static async Task<byte[]> ReadToEndAsync(NetworkStream stream)
        {
            using var cts = new CancellationTokenSource(Timeout);
            var received = new MemoryStream();
            var buffer = new byte[16384];
            int read;
            while ((read = await stream.ReadAsync(buffer, cts.Token)) > 0)
            {
                received.Write(buffer, 0, read);
            }
            return received.ToArray();
        }

        private static async Task<byte[]> ReadExactlyAsync(NetworkStream stream, int length)
        {
            using var cts = new CancellationTokenSource(Timeout);
            var buffer = new byte[length];
            await stream.ReadExactlyAsync(buffer, cts.Token);
            return buffer;
        }

        private static async Task WriteInPiecesAsync(NetworkStream stream, byte[] data, int seed)
        {
            var random = new Random(seed);
            int position = 0;
            while (position < data.Length)
            {
                int size = Math.Min(random.Next(1, 40000), data.Length - position);
                await stream.WriteAsync(data.AsMemory(position, size));
                position += size;
            }
        }

        [Fact]
        public async Task Echo_ReturnsEveryByteInOrder()
        {
            using var server = TestServer.Echo();
            using var tunnel = new LoopbackTunnel(server.Port);
            using var client = tunnel.ConnectClient();
            var stream = client.GetStream();

            var data = RandomBytes(4 * 1024 * 1024, 1);
            var writer = WriteInPiecesAsync(stream, data, 11);

            var echoed = await ReadExactlyAsync(stream, data.Length);
            await writer;

            Assert.True(data.AsSpan().SequenceEqual(echoed), "the echoed bytes differ");
        }

        [Fact]
        public async Task ManyConnectionsAtOnce_DoNotMixTheirBytes()
        {
            using var server = TestServer.Echo();
            using var tunnel = new LoopbackTunnel(server.Port);

            var transfers = Enumerable.Range(0, 24).Select(async index =>
            {
                using var client = tunnel.ConnectClient();
                var stream = client.GetStream();
                var data = RandomBytes(300_000 + index * 1000, 100 + index);

                var writer = WriteInPiecesAsync(stream, data, index);
                var echoed = await ReadExactlyAsync(stream, data.Length);
                await writer;

                return data.AsSpan().SequenceEqual(echoed);
            }).ToArray();

            Assert.All(await Task.WhenAll(transfers), Assert.True);
        }

        [Fact]
        public async Task ServerSendsEverythingThenCloses_ClientReceivesAllOfIt()
        {
            //The server closes the moment it has written the last byte, so the disconnect races the last packets through the tunnel.
            var payloads = Enumerable.Range(0, 30).Select(i => RandomBytes(200_000 + i * 7919, 500 + i)).ToList();
            int next = -1;

            using var server = new TestServer(async (client, token) =>
            {
                var data = payloads[Interlocked.Increment(ref next)];
                await client.GetStream().WriteAsync(data, token);
                //No read, no delay: the server socket is closed as soon as this returns.
            });
            using var tunnel = new LoopbackTunnel(server.Port);

            var results = new List<(int Expected, int Actual)>();

            //Connect one at a time so that connection i gets payload i.
            for (int i = 0; i < payloads.Count; i++)
            {
                using var client = tunnel.ConnectClient();
                var received = await ReadToEndAsync(client.GetStream());
                Assert.True(payloads[i].AsSpan().SequenceEqual(received), $"connection {i}: expected {payloads[i].Length} bytes, received {received.Length}");
            }
        }

        [Fact]
        public async Task ClientSendsEverythingThenCloses_ServerReceivesAllOfIt()
        {
            var received = new System.Collections.Concurrent.ConcurrentDictionary<int, byte[]>();
            int connection = -1;

            using var server = new TestServer(async (client, token) =>
            {
                int index = Interlocked.Increment(ref connection);
                var memory = new MemoryStream();
                await client.GetStream().CopyToAsync(memory, token);
                received[index] = memory.ToArray();
            });
            using var tunnel = new LoopbackTunnel(server.Port);

            var sent = new List<byte[]>();
            for (int i = 0; i < 20; i++)
            {
                var data = RandomBytes(150_000 + i * 3571, 900 + i);
                sent.Add(data);

                using var client = tunnel.ConnectClient();
                await WriteInPiecesAsync(client.GetStream(), data, i);
                client.Close(); //The last bytes are followed at once by the disconnect.

                //Wait for the server to have seen the end of this connection before starting the next, so that indexes line up.
                var deadline = DateTime.UtcNow + Timeout;
                while (!received.ContainsKey(i) && DateTime.UtcNow < deadline)
                {
                    await Task.Delay(5);
                }

                Assert.True(received.TryGetValue(i, out var actual), $"connection {i}: the server never saw the end");
                Assert.True(data.AsSpan().SequenceEqual(actual), $"connection {i}: sent {data.Length} bytes, the server received {actual.Length}");
            }
        }

        [Fact]
        public async Task HttpHeaderRule_IsAppliedToRequestsThatTravelTheTunnel()
        {
            var request = new TaskCompletionSource<string>();

            using var server = new TestServer(async (client, token) =>
            {
                var stream = client.GetStream();
                var buffer = new byte[4096];
                var text = new StringBuilder();
                while (!text.ToString().Contains("\r\n\r\n"))
                {
                    int read = await stream.ReadAsync(buffer, token);
                    if (read == 0) break;
                    text.Append(Encoding.Latin1.GetString(buffer, 0, read));
                }
                request.TrySetResult(text.ToString());

                await stream.WriteAsync("HTTP/1.1 200 OK\r\nContent-Length: 5\r\nConnection: close\r\n\r\nhello"u8.ToArray(), token);
            });

            using var tunnel = new LoopbackTunnel(server.Port, configuration =>
            {
                configuration.InboundTrafficType = NtTrafficType.Http;
                configuration.OutboundTrafficType = NtTrafficType.Http;
                configuration.HttpHeaderRules.Add(new HttpHeaderRule(NtHttpVerb.Any, "X-Forwarded-For", NtHttpHeaderAction.Upsert, "10.1.2.3")
                { HeaderType = NtHttpHeaderType.Request, Enabled = true });
            });

            using var client = tunnel.ConnectClient();
            await client.GetStream().WriteAsync("GET /index.html HTTP/1.1\r\nHost: example.com\r\n\r\n"u8.ToArray());

            var response = Encoding.Latin1.GetString(await ReadToEndAsync(client.GetStream()));

            Assert.Contains("X-Forwarded-For: 10.1.2.3", await request.Task.WaitAsync(Timeout));
            Assert.EndsWith("hello", response);
        }

        [Fact]
        public async Task EndpointWhosePortIsBusy_StartsListeningOnceItIsFree()
        {
            using var server = TestServer.Echo();
            using var tunnel = new LoopbackTunnel(server.Port, start: false);

            //Another program holds the port when the endpoint starts.
            var blocker = new TcpListener(System.Net.IPAddress.Any, tunnel.ListenPort);
            blocker.Start();

            tunnel.Inbound.Start();
            await Task.Delay(500);
            blocker.Stop();

            //It keeps trying (every few seconds) instead of staying "running" without listening.
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(20);
            TcpClient? client = null;
            while (client == null && DateTime.UtcNow < deadline)
            {
                try
                {
                    client = tunnel.ConnectClient();
                }
                catch (SocketException)
                {
                    await Task.Delay(250);
                }
            }

            Assert.NotNull(client);
            using (client)
            {
                var stream = client.GetStream();
                await stream.WriteAsync("ping"u8.ToArray());
                Assert.Equal("ping", Encoding.ASCII.GetString(await ReadExactlyAsync(stream, 4)));
            }
        }
    }
}
