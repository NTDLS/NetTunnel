using System.Buffers.Binary;
using System.Text;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.TunnelEngine.Http2;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    public class Http2Tests
    {
        private const byte Data = 0, Headers = 1, Settings = 4, Continuation = 9;
        private const byte EndStream = 1, EndHeaders = 4, Padded = 8, Priority = 0x20;

        private static readonly byte[] Preface = Encoding.ASCII.GetBytes("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n");

        private static EndpointConfiguration Config(NtHttpHeaderType type, params HttpHeaderRule[] rules)
        {
            var config = new EndpointConfiguration
            {
                Direction = type == NtHttpHeaderType.Request ? NtDirection.Inbound : NtDirection.Outbound,
                InboundTrafficType = NtTrafficType.Https,
                OutboundTrafficType = NtTrafficType.Https
            };
            config.HttpHeaderRules.AddRange(rules);
            return config;
        }

        private static HttpHeaderRule Rule(NtHttpHeaderType type, NtHttpHeaderAction action, string name, string value, NtHttpVerb verb = NtHttpVerb.Any)
            => new(verb, name, action, value) { HeaderType = type, Enabled = true };

        #region Frame building.

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

        private static byte[] Concat(params byte[][] parts) => parts.SelectMany(o => o).ToArray();

        private static byte[] Literal(string name, string value, bool incrementalIndexing = false)
            => Concat([(byte)(incrementalIndexing ? 0x40 : 0x00)], Str(name), Str(value));

        private static byte[] Str(string value)
        {
            var bytes = Encoding.Latin1.GetBytes(value);
            Assert.True(bytes.Length < 127);
            return Concat([(byte)bytes.Length], bytes);
        }

        /// <summary>A literal with incremental indexing, whose name is the given static table index.</summary>
        private static byte[] IndexedNameLiteral(int staticIndex, string value)
            => Concat([(byte)(0x40 | staticIndex)], Str(value));

        private static byte[] Indexed(int index) => [(byte)(0x80 | index)];

        private static byte[] RequestBlock(string authority, string path = "/")
            => Concat(Indexed(2), Indexed(7), Indexed(path == "/" ? 4 : 0 + 4), IndexedNameLiteral(1, authority));

        #endregion

        /// <summary>Everything emitted for the stream when fed in pieces of the given sizes.</summary>
        private static byte[] Run(Http2StreamParser parser, byte[] input, params int[] pieceSizes)
        {
            var output = new MemoryStream();
            int position = 0, piece = 0;
            while (position < input.Length)
            {
                int size = pieceSizes.Length == 0 ? input.Length : pieceSizes[Math.Min(piece++, pieceSizes.Length - 1)];
                size = Math.Min(size, input.Length - position);

                var chunk = new byte[size + 5];
                Array.Copy(input, position, chunk, 0, size);
                var (result, length) = parser.Process(chunk, size);
                output.Write(result, 0, length);
                position += size;
            }
            return output.ToArray();
        }

        private record ParsedFrame(byte Type, byte Flags, uint StreamId, byte[] Payload);

        private static List<ParsedFrame> ParseFrames(byte[] stream, bool hasPreface)
        {
            int position = 0;
            if (hasPreface)
            {
                Assert.True(stream.AsSpan(0, Preface.Length).SequenceEqual(Preface));
                position = Preface.Length;
            }

            var frames = new List<ParsedFrame>();
            while (position < stream.Length)
            {
                Assert.True(stream.Length - position >= 9, "truncated frame header");
                int length = (stream[position] << 16) | (stream[position + 1] << 8) | stream[position + 2];
                Assert.True(stream.Length - position - 9 >= length, "truncated frame payload");
                frames.Add(new ParsedFrame(stream[position + 3], stream[position + 4],
                    BinaryPrimitives.ReadUInt32BigEndian(stream.AsSpan(position + 5)) & 0x7FFFFFFF,
                    stream.AsSpan(position + 9, length).ToArray()));
                position += 9 + length;
            }
            return frames;
        }

        /// <summary>Decodes the header blocks of the stream with an independent decoder, as a real receiver would.</summary>
        private static List<List<(string Name, string Value)>> DecodeBlocks(byte[] stream, bool hasPreface)
        {
            var decoder = new HpackDecoder();
            var results = new List<List<(string, string)>>();
            var block = new MemoryStream();

            foreach (var frame in ParseFrames(stream, hasPreface))
            {
                if (frame.Type != Headers && frame.Type != Continuation)
                {
                    continue;
                }

                var payload = frame.Payload.AsSpan();
                if (frame.Type == Headers)
                {
                    Assert.Equal(0, frame.Flags & Padded); //Output never pads.
                    if ((frame.Flags & Priority) != 0)
                    {
                        payload = payload[5..];
                    }
                    block.SetLength(0);
                }
                block.Write(payload);

                if ((frame.Flags & EndHeaders) != 0)
                {
                    results.Add(decoder.Decode(block.ToArray()).Select(o => (o.NameText, o.ValueText)).ToList());
                }
            }
            return results;
        }

        private static (byte[] Stream, int Blocks) ClientStream(params string[] authorities)
        {
            var parts = new List<byte[]> { Preface, Frame(Settings, 0, 0, []) };
            uint stream = 1;
            foreach (var authority in authorities)
            {
                parts.Add(Frame(Headers, EndHeaders | EndStream, stream, RequestBlock(authority)));
                stream += 2;
            }
            return (Concat([.. parts]), authorities.Length);
        }

        [Fact]
        public void NonHeaderFrames_ArePassedOnByteForByte()
        {
            var settings = Frame(Settings, 0, 0, [0, 3, 0, 0, 0, 100, 0, 4, 0, 1, 0, 0]);
            var data = Frame(Data, 0, 1, Enumerable.Range(0, 5000).Select(o => (byte)(o * 7)).ToArray());
            var ping = Frame(6, 0, 0, [1, 2, 3, 4, 5, 6, 7, 8]);
            var input = Concat(Preface, settings, data, ping, Frame(Data, EndStream, 1, []));

            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            Assert.Equal(input, Run(new Http2StreamParser(config, true), input));
            Assert.Equal(input, Run(new Http2StreamParser(config, true), input, 1));
            Assert.Equal(input, Run(new Http2StreamParser(config, true), input, 13, 4096));
        }

        [Fact]
        public void RuleApplied_AndOutputIsIndependentOfSegmentation()
        {
            var (input, count) = ClientStream("a.example", "b.example", "c.example");
            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));

            var expected = Run(new Http2StreamParser(config, true), input);
            var blocks = DecodeBlocks(expected, true);
            Assert.Equal(count, blocks.Count);

            foreach (var (block, authority) in blocks.Zip(new[] { "a.example", "b.example", "c.example" }))
            {
                Assert.Contains((":authority", authority), block);
                Assert.Contains(("x-test", "1"), block);
                Assert.Contains((":method", "GET"), block);
            }

            Assert.Equal(expected, Run(new Http2StreamParser(config, true), input, 1));
            for (int split = 1; split < input.Length; split++)
            {
                Assert.Equal(expected, Run(new Http2StreamParser(config, true), input, split, input.Length));
            }
        }

        [Fact]
        public void HostRule_IsAppliedToAuthority()
        {
            var (input, _) = ClientStream("a.example");
            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "Host", "rewritten"));

            var block = DecodeBlocks(Run(new Http2StreamParser(config, true), input), true).Single();
            Assert.Contains((":authority", "rewritten"), block);
            Assert.DoesNotContain(block, f => f.Name == "host");
        }

        [Fact]
        public void DynamicTable_IsHonoredAcrossBlocks()
        {
            //Request 1 adds custom-key to the dynamic table, request 2 refers to it by index (62).
            var base1 = Concat(Indexed(2), Indexed(6), Indexed(4), Literal("custom-key", "custom-value", incrementalIndexing: true));
            var base2 = Concat(Indexed(2), Indexed(6), Indexed(4), Indexed(62));
            var input = Concat(Preface, Frame(Headers, EndHeaders | EndStream, 1, base1), Frame(Headers, EndHeaders | EndStream, 3, base2));

            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var blocks = DecodeBlocks(Run(new Http2StreamParser(config, true), input, 1), true);

            Assert.Equal(2, blocks.Count);
            Assert.Contains(("custom-key", "custom-value"), blocks[0]);
            Assert.Contains(("custom-key", "custom-value"), blocks[1]);
        }

        [Fact]
        public void Rule_DoesNotLeakIntoLaterRequests_ThroughTheDynamicTable()
        {
            //A POST-only rule overwrites user-agent. The GET that follows reuses the dynamic table entry that the POST created.
            var ua = Literal("user-agent", "Chrome", incrementalIndexing: true);
            var post = Concat(Indexed(3), Indexed(7), Indexed(4), ua);
            var get = Concat(Indexed(2), Indexed(7), Indexed(4), Indexed(62));
            var input = Concat(Preface, Frame(Headers, EndHeaders | EndStream, 1, post), Frame(Headers, EndHeaders | EndStream, 3, get));

            var config = Config(NtHttpHeaderType.Request,
                Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "user-agent", "Rewritten", NtHttpVerb.Post));
            var blocks = DecodeBlocks(Run(new Http2StreamParser(config, true), input), true);

            Assert.Contains(("user-agent", "Rewritten"), blocks[0]);
            Assert.Contains(("user-agent", "Chrome"), blocks[1]);
        }

        [Fact]
        public void Rule_DoesNotChangeTheSharedStaticTable()
        {
            //":scheme https" is static table entry 7. A rule that rewrites it must not change what other connections decode.
            var input = Concat(Preface, Frame(Headers, EndHeaders | EndStream, 1, Concat(Indexed(2), Indexed(7), Indexed(4))));
            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Update, ":scheme", "http"));

            var blocks = DecodeBlocks(Run(new Http2StreamParser(config, true), input), true);
            Assert.Contains((":scheme", "http"), blocks[0]);

            var untouched = new HpackDecoder().Decode(Indexed(7));
            Assert.Equal("https", untouched.Single().ValueText);
        }

        [Fact]
        public void HeadersWithPaddingAndPriority_AreReencodedCorrectly()
        {
            var fragment = RequestBlock("a.example");
            var priority = new byte[] { 0x80, 0, 0, 0, 15 };
            var payload = Concat([(byte)4], priority, fragment, new byte[4]); //Pad length 4.
            var input = Concat(Preface, Frame(Headers, EndHeaders | EndStream | Padded | Priority, 1, payload));

            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var output = Run(new Http2StreamParser(config, true), input, 3);
            var frame = ParseFrames(output, true).Single();

            Assert.Equal(Headers, frame.Type);
            Assert.NotEqual(0, frame.Flags & Priority);
            Assert.Equal(0, frame.Flags & Padded);
            Assert.Equal(EndStream, frame.Flags & EndStream);
            Assert.Equal(priority, frame.Payload[..5]);
            Assert.Contains(("x-test", "1"), DecodeBlocks(output, true).Single());
        }

        [Fact]
        public void InputContinuationFrames_AreJoined()
        {
            var block = RequestBlock("a.example");
            var input = Concat(Preface,
                Frame(Headers, 0, 1, block[..3]),
                Frame(Continuation, 0, 1, block[3..7]),
                Frame(Continuation, EndHeaders, 1, block[7..]));

            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var expected = Run(new Http2StreamParser(config, true), input);

            var decoded = DecodeBlocks(expected, true).Single();
            Assert.Contains((":authority", "a.example"), decoded);
            Assert.Contains(("x-test", "1"), decoded);

            for (int split = 1; split < input.Length; split++)
            {
                Assert.Equal(expected, Run(new Http2StreamParser(config, true), input, split, input.Length));
            }
        }

        [Fact]
        public void OversizedHeaderBlock_IsSplitIntoContinuationFrames_NoneOverTheDefaultMax()
        {
            var big = new string('v', 100);
            var fields = Concat(Indexed(2), Indexed(7), Indexed(4), IndexedNameLiteral(1, "a.example"));
            for (int i = 0; i < 400; i++) //~44KB of header data.
            {
                fields = Concat(fields, Literal($"x-header-{i}", big));
            }

            var input = Concat(Preface, Frame(Headers, EndStream, 1, fields[..10]), Frame(Continuation, EndHeaders, 1, fields[10..]));
            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var output = Run(new Http2StreamParser(config, true), input, 1000);

            var frames = ParseFrames(output, true);
            Assert.True(frames.Count > 1);
            Assert.All(frames, f => Assert.True(f.Payload.Length <= 16384));
            Assert.Equal(Headers, frames[0].Type);
            Assert.All(frames.Skip(1), f => Assert.Equal(Continuation, f.Type));
            Assert.Equal(0, frames[0].Flags & EndHeaders);
            Assert.Equal(EndHeaders, frames[^1].Flags & EndHeaders);

            var decoded = DecodeBlocks(output, true).Single();
            Assert.Equal(400 + 4 + 1, decoded.Count);
            Assert.Contains(("x-header-399", big), decoded);
        }

        [Fact]
        public void Trailers_AreNotGivenTheRules()
        {
            var request = Concat(Indexed(3), Indexed(7), Indexed(4), IndexedNameLiteral(1, "a.example"));
            var trailers = Literal("x-trailer", "t");
            var input = Concat(Preface,
                Frame(Headers, EndHeaders, 1, request),
                Frame(Data, 0, 1, [1, 2, 3]),
                Frame(Headers, EndHeaders | EndStream, 1, trailers));

            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var blocks = DecodeBlocks(Run(new Http2StreamParser(config, true), input), true);

            Assert.Contains(("x-test", "1"), blocks[0]);
            Assert.DoesNotContain(("x-test", "1"), blocks[1]);
            Assert.Contains(("x-trailer", "t"), blocks[1]);
        }

        [Fact]
        public void Responses_InformationalIsSkipped_FinalGetsRules()
        {
            var info = Concat(Literal(":status", "103"));
            var final = Concat(Indexed(8), Literal("content-type", "text/plain"));
            var input = Concat(Frame(Headers, EndHeaders, 1, info), Frame(Headers, EndHeaders, 1, final), Frame(Data, EndStream, 1, [9, 9]));

            var config = Config(NtHttpHeaderType.Response, Rule(NtHttpHeaderType.Response, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var blocks = DecodeBlocks(Run(new Http2StreamParser(config, false), input), false);

            Assert.DoesNotContain(("x-test", "1"), blocks[0]);
            Assert.Contains(("x-test", "1"), blocks[1]);
        }

        [Fact]
        public void HuffmanEncodedStrings_AreDecoded()
        {
            //RFC 7541 C.4.1: ":authority: www.example.com" Huffman encoded.
            byte[] block = [0x82, 0x86, 0x84, 0x41, 0x8c, 0xf1, 0xe3, 0xc2, 0xe5, 0xf2, 0x3a, 0x6b, 0xa0, 0xab, 0x90, 0xf4, 0xff];
            var input = Concat(Preface, Frame(Headers, EndHeaders | EndStream, 1, block));

            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var decoded = DecodeBlocks(Run(new Http2StreamParser(config, true), input), true).Single();

            Assert.Contains((":authority", "www.example.com"), decoded);
            Assert.Contains((":method", "GET"), decoded);
        }

        [Fact]
        public void Rfc7541_C3_RequestsWithoutHuffman_DecodeToTheDocumentedFields()
        {
            var decoder = new HpackDecoder();
            var first = decoder.Decode(Convert.FromHexString("828684410f7777772e6578616d706c652e636f6d"));
            Assert.Equal([":method=GET", ":scheme=http", ":path=/", ":authority=www.example.com"], first.Select(o => $"{o.NameText}={o.ValueText}"));

            var second = decoder.Decode(Convert.FromHexString("828684be58086e6f2d6361636865"));
            Assert.Equal([":method=GET", ":scheme=http", ":path=/", ":authority=www.example.com", "cache-control=no-cache"],
                second.Select(o => $"{o.NameText}={o.ValueText}"));

            var third = decoder.Decode(Convert.FromHexString("828785bf400a637573746f6d2d6b65790c637573746f6d2d76616c7565"));
            Assert.Equal([":method=GET", ":scheme=https", ":path=/index.html", ":authority=www.example.com", "custom-key=custom-value"],
                third.Select(o => $"{o.NameText}={o.ValueText}"));
        }

        [Fact]
        public void EncoderOutput_RoundTripsAnyFieldBytes()
        {
            var random = new Random(1234);
            var fields = new List<HeaderField>();
            for (int i = 0; i < 300; i++)
            {
                var name = new byte[random.Next(1, 200)];
                var value = new byte[random.Next(0, 3000)];
                random.NextBytes(name);
                random.NextBytes(value);
                fields.Add(new HeaderField(name, value, neverIndex: i % 7 == 0));
            }
            fields.Add(new HeaderField(":method", "GET"));
            fields.Add(new HeaderField("content-length", ""));

            var decoded = new HpackDecoder().Decode(HpackEncoder.Encode(fields));
            Assert.Equal(fields.Count, decoded.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                Assert.Equal(fields[i].Name, decoded[i].Name);
                Assert.Equal(fields[i].Value, decoded[i].Value);
                Assert.Equal(fields[i].NeverIndex, decoded[i].NeverIndex);
            }
        }

        [Fact]
        public void NotHttp2_ThrowsRatherThanForwardingGarbage()
        {
            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var parser = new Http2StreamParser(config, true);
            Assert.Throws<InvalidDataException>(() => parser.Process(Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\n\r\n"), 18));
        }

        [Fact]
        public void BadHpack_ThrowsRatherThanForwardingGarbage()
        {
            var config = Config(NtHttpHeaderType.Request, Rule(NtHttpHeaderType.Request, NtHttpHeaderAction.Upsert, "x-test", "1"));
            var input = Concat(Preface, Frame(Headers, EndHeaders, 1, [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]));
            Assert.ThrowsAny<Exception>(() => Run(new Http2StreamParser(config, true), input));
        }
    }
}
