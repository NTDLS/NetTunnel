using System.Text;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.TunnelEngine;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    public class Http1ParserTests
    {
        //Direction must match what is parsed: a mismatch logs a warning, which loads the service configuration from disk.
        private static EndpointConfiguration Config(params HttpHeaderRule[] rules)
        {
            bool responses = rules.Length > 0 && rules.All(o => o.HeaderType == NtHttpHeaderType.Response);
            var config = new EndpointConfiguration
            {
                Direction = responses ? NtDirection.Outbound : NtDirection.Inbound,
                InboundTrafficType = NtTrafficType.Http,
                OutboundTrafficType = NtTrafficType.Http
            };
            config.HttpHeaderRules.AddRange(rules);
            return config;
        }

        private static HttpHeaderRule Rule(NtHttpHeaderType type, NtHttpHeaderAction action, string name, string value, NtHttpVerb verb = NtHttpVerb.Any)
            => new(verb, name, action, value) { HeaderType = type, Enabled = true };

        /// <summary>
        /// Feeds the stream to a new parser in pieces of the given sizes (repeating the last) and returns everything it emitted.
        /// </summary>
        private static string Run(EndpointConfiguration config, string input, params int[] pieceSizes)
        {
            var parser = new HttpStreamParser(config);
            var bytes = Encoding.Latin1.GetBytes(input);
            var output = new MemoryStream();

            int position = 0, piece = 0;
            while (position < bytes.Length)
            {
                int size = pieceSizes.Length == 0 ? bytes.Length : pieceSizes[Math.Min(piece++, pieceSizes.Length - 1)];
                size = Math.Min(size, bytes.Length - position);

                var chunk = new byte[size + 7]; //Larger than the data, like a real receive buffer.
                Array.Copy(bytes, position, chunk, 0, size);
                var (result, length) = parser.Process(chunk, size);
                output.Write(result, 0, length);
                position += size;
            }

            return Encoding.Latin1.GetString(output.ToArray());
        }

        /// <summary>
        /// The output must not depend on how the stream happened to be segmented by the network.
        /// </summary>
        private static void AssertSplitInvariant(EndpointConfiguration config, string input, string expected)
        {
            Assert.Equal(expected, Run(config, input));
            Assert.Equal(expected, Run(config, input, 1));
            for (int split = 1; split < input.Length; split++)
            {
                Assert.Equal(expected, Run(config, input, split, input.Length));
            }
        }

        private static readonly HttpHeaderRule AddTest = Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "X-Test", "1");

        [Fact]
        public void NoBody_RuleAppliedToEveryPipelinedRequest()
        {
            var input = "GET /a HTTP/1.1\r\nHost: x\r\n\r\nGET /b HTTP/1.1\r\nHost: x\r\n\r\n";
            var expected = "GET /a HTTP/1.1\r\nHost: x\r\nX-Test: 1\r\n\r\nGET /b HTTP/1.1\r\nHost: x\r\nX-Test: 1\r\n\r\n";
            AssertSplitInvariant(Config(AddTest), input, expected);
        }

        [Fact]
        public void ContentLengthBody_IsNotMistakenForHeader()
        {
            //The body looks exactly like a request that the rule would otherwise modify.
            var body = "GET /evil HTTP/1.1\r\nHost: y\r\n\r\n";
            var input = $"POST /a HTTP/1.1\r\nContent-Length: {body.Length}\r\n\r\n{body}GET /b HTTP/1.1\r\n\r\n";
            var expected = $"POST /a HTTP/1.1\r\nContent-Length: {body.Length}\r\nX-Test: 1\r\n\r\n{body}GET /b HTTP/1.1\r\nX-Test: 1\r\n\r\n";
            AssertSplitInvariant(Config(AddTest), input, expected);
        }

        [Fact]
        public void ChunkedBody_WithExtensionsAndTrailers()
        {
            var input = "POST /a HTTP/1.1\r\nTransfer-Encoding: chunked\r\n\r\n"
                + "5;ext=1\r\nhello\r\n6\r\n GET /\r\n0\r\nTrailer-X: y\r\n\r\n"
                + "GET /b HTTP/1.1\r\n\r\n";
            var expected = "POST /a HTTP/1.1\r\nTransfer-Encoding: chunked\r\nX-Test: 1\r\n\r\n"
                + "5;ext=1\r\nhello\r\n6\r\n GET /\r\n0\r\nTrailer-X: y\r\n\r\n"
                + "GET /b HTTP/1.1\r\nX-Test: 1\r\n\r\n";
            AssertSplitInvariant(Config(AddTest), input, expected);
        }

        [Fact]
        public void LfOnlyLineEndings()
        {
            var input = "GET /a HTTP/1.1\nHost: x\n\nGET /b HTTP/1.1\nHost: x\n\n";
            var expected = "GET /a HTTP/1.1\nHost: x\nX-Test: 1\n\nGET /b HTTP/1.1\nHost: x\nX-Test: 1\n\n";
            AssertSplitInvariant(Config(AddTest), input, expected);
        }

        [Fact]
        public void Rules_UpsertUpdateInsertDelete()
        {
            var config = Config(
                Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "Host", "new"),
                Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Update, "Missing", "v"),
                Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Insert, "User-Agent", "other"),
                Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Delete, "Cookie", ""));

            var input = "GET / HTTP/1.1\r\nHost: old\r\nUser-Agent: keep\r\nCookie: a=b\r\nAccept: */*\r\n\r\n";
            var expected = "GET / HTTP/1.1\r\nHost: new\r\nUser-Agent: keep\r\nAccept: */*\r\n\r\n";
            AssertSplitInvariant(config, input, expected);
        }

        [Fact]
        public void VerbSpecificRule_OnlyAppliesToThatVerb()
        {
            var config = Config(Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "X-Test", "1", NtHttpVerb.Post));
            var input = "GET /a HTTP/1.1\r\n\r\nPOST /b HTTP/1.1\r\n\r\n";
            var expected = "GET /a HTTP/1.1\r\n\r\nPOST /b HTTP/1.1\r\nX-Test: 1\r\n\r\n";
            AssertSplitInvariant(config, input, expected);
        }

        [Fact]
        public void Response_NoBodyStatuses_AndContentLength()
        {
            var config = Config(Rule(NtHttpHeaderType.Response, NtHttpHeaderAction.Upsert, "X-Test", "1"));
            var input = "HTTP/1.1 100 Continue\r\n\r\nHTTP/1.1 204 No Content\r\n\r\nHTTP/1.1 304 Not Modified\r\nContent-Length: 99\r\n\r\n"
                + "HTTP/1.1 200 OK\r\nContent-Length: 4\r\n\r\nHTTP" + "HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n";
            var expected = "HTTP/1.1 100 Continue\r\nX-Test: 1\r\n\r\nHTTP/1.1 204 No Content\r\nX-Test: 1\r\n\r\nHTTP/1.1 304 Not Modified\r\nContent-Length: 99\r\nX-Test: 1\r\n\r\n"
                + "HTTP/1.1 200 OK\r\nContent-Length: 4\r\nX-Test: 1\r\n\r\nHTTP" + "HTTP/1.1 200 OK\r\nContent-Length: 0\r\nX-Test: 1\r\n\r\n";
            AssertSplitInvariant(config, input, expected);
        }

        [Fact]
        public void Response_101_SwitchesToPassthrough()
        {
            var config = Config(Rule(NtHttpHeaderType.Response, NtHttpHeaderAction.Upsert, "X-Test", "1"));
            var input = "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\n\r\nGET / HTTP/1.1\r\n\r\n\u0081\u0005hello";
            var expected = "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nX-Test: 1\r\n\r\nGET / HTTP/1.1\r\n\r\n\u0081\u0005hello";
            AssertSplitInvariant(config, input, expected);
        }

        [Fact]
        public void NonHttpData_PassesThroughUnchanged()
        {
            var input = "\u0016\u0003\u0001\u0002\u0000 binary stuff \r\n\r\n";
            AssertSplitInvariant(Config(AddTest), input, input);
        }

        [Fact]
        public void CaseInsensitiveVerbAndHeaderName()
        {
            var config = Config(Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "host", "new"));
            var input = "get / HTTP/1.1\r\nHOST: old\r\n\r\n";
            var expected = "get / HTTP/1.1\r\nhost: new\r\n\r\n";
            AssertSplitInvariant(config, input, expected);
        }

        [Fact]
        public void ProtocolText_IsNotAffectedByTheMachinesCulture()
        {
            //In Turkish, "i" upper-cases to a dotted capital I, which broke verb matching ("options" is not "OPTIONS").
            var original = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");

                var config = Config(Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "X-Test", "1", NtHttpVerb.Options));
                var input = "options * http/1.1\r\nhost: x\r\n\r\nOPTIONS * HTTP/1.1\r\n\r\n";
                var expected = "options * http/1.1\r\nhost: x\r\nX-Test: 1\r\n\r\nOPTIONS * HTTP/1.1\r\nX-Test: 1\r\n\r\n";
                AssertSplitInvariant(config, input, expected);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void HeaderBytesAbove127_AreNotAltered()
        {
            var input = "GET /café HTTP/1.1\r\nX-Raw: ÿ\u0080©\r\n\r\n";
            var expected = "GET /café HTTP/1.1\r\nX-Raw: ÿ\u0080©\r\nX-Test: 1\r\n\r\n";
            AssertSplitInvariant(Config(AddTest), input, expected);
        }

        [Fact]
        public void LargeBody_SpanningManyBuffers()
        {
            var body = new string('x', 300_000);
            var input = $"POST / HTTP/1.1\r\nContent-Length: {body.Length}\r\n\r\n{body}GET / HTTP/1.1\r\n\r\n";
            var expected = $"POST / HTTP/1.1\r\nContent-Length: {body.Length}\r\nX-Test: 1\r\n\r\n{body}GET / HTTP/1.1\r\nX-Test: 1\r\n\r\n";
            Assert.Equal(expected, Run(Config(AddTest), input, 4096));
            Assert.Equal(expected, Run(Config(AddTest), input, 70_000, 1, 99_999));
        }

        [Fact]
        public void MixedFramingBackToBack_SplitAtEveryByte()
        {
            var input = "POST /a HTTP/1.1\r\nContent-Length: 3\r\n\r\nabcPOST /b HTTP/1.1\r\nTransfer-Encoding: chunked\r\n\r\n2\r\nab\r\n0\r\n\r\n";
            var expected = "POST /a HTTP/1.1\r\nContent-Length: 3\r\nX-Test: 1\r\n\r\nabcPOST /b HTTP/1.1\r\nTransfer-Encoding: chunked\r\nX-Test: 1\r\n\r\n2\r\nab\r\n0\r\n\r\n";
            AssertSplitInvariant(Config(AddTest), input, expected);
        }
    }
}
