using System.Text;
using System.Text.RegularExpressions;
using NetTunnel.Service;
using NetTunnel.Service.TunnelEngine;

namespace NetTunnel.Tests
{
    /// <summary>
    /// The header rules used to be regular expressions and are now a plain scan of the header text (they run on every message, so the
    /// cost matters). This keeps the original expressions as a reference and compares the two on a large number of generated headers.
    /// </summary>
    public class HttpRulesDifferentialTests
    {
        #region The original implementation, unchanged.

        private static Regex HeaderLineFinder(string name, bool includeLineBreak)
            => new(@"^" + Regex.Escape(name) + @"[ \t]*:[^\r\n]*" + (includeLineBreak ? @"(\r?\n)?" : string.Empty),
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static string ReferenceAppend(string headerText, string name, string value, string lineBreak)
        {
            var newLine = $"{name}: {value}{lineBreak}";
            if (headerText.EndsWith(lineBreak + lineBreak))
            {
                return headerText.Insert(headerText.Length - lineBreak.Length, newLine);
            }
            return headerText + newLine;
        }

        private static string ReferenceInsert(string headerText, string name, string value, string lineBreak)
            => HeaderLineFinder(name, false).IsMatch(headerText) ? headerText : ReferenceAppend(headerText, name, value, lineBreak);

        private static string ReferenceUpsert(string headerText, string name, string value, string lineBreak)
        {
            var finder = HeaderLineFinder(name, false);
            return finder.IsMatch(headerText) ? finder.Replace(headerText, _ => $"{name}: {value}") : ReferenceAppend(headerText, name, value, lineBreak);
        }

        private static string ReferenceUpdate(string headerText, string name, string value)
            => HeaderLineFinder(name, false).Replace(headerText, _ => $"{name}: {value}");

        private static string ReferenceDelete(string headerText, string name)
            => HeaderLineFinder(name, true).Replace(headerText, string.Empty);

        private static readonly Regex ContentLengthFinder =
            new(@"^Content-Length[ \t]*:[ \t]*(\d+)[ \t]*\r?$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex TransferEncodingFinder =
            new(@"^Transfer-Encoding[ \t]*:([^\r\n]*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        #endregion

        private static readonly string[] Names = ["Host", "X-Test", "Content-Length", "Transfer-Encoding", "Cookie", "User-Agent", "Accept"];

        private static string RandomHeader(Random random, out string lineBreak)
        {
            lineBreak = random.Next(3) == 0 ? "\n" : "\r\n";
            var text = new StringBuilder();
            text.Append(random.Next(2) == 0 ? "GET /a/b?c=d HTTP/1.1" : "HTTP/1.1 200 OK").Append(RandomBreak(random, lineBreak));

            int lines = random.Next(0, 9);
            for (int i = 0; i < lines; i++)
            {
                switch (random.Next(10))
                {
                    case 0: //Junk without a colon.
                        text.Append("not a header line");
                        break;
                    case 1: //Starts like a name that is longer.
                        text.Append(Names[random.Next(Names.Length)]).Append("-Extra: v");
                        break;
                    case 2: //Empty value.
                        text.Append(RandomCase(random, Names[random.Next(Names.Length)])).Append(':');
                        break;
                    default:
                        var name = RandomCase(random, Names[random.Next(Names.Length)]);
                        text.Append(name)
                            .Append(random.Next(4) == 0 ? " \t" : string.Empty)
                            .Append(':')
                            .Append(random.Next(4) == 0 ? string.Empty : random.Next(2) == 0 ? " " : "\t ")
                            .Append(random.Next(3) == 0 ? random.Next(0, 100000).ToString() : "value " + random.Next(100))
                            .Append(random.Next(8) == 0 ? "  " : string.Empty)
                            .Append(random.Next(30) == 0 ? "\rjunk" : string.Empty);
                        break;
                }
                text.Append(RandomBreak(random, lineBreak));
            }

            text.Append(random.Next(12) == 0 ? string.Empty : lineBreak); //Usually terminated by a blank line.
            return text.ToString();
        }

        private static string RandomBreak(Random random, string lineBreak) => random.Next(25) == 0 ? (lineBreak == "\n" ? "\r\n" : "\n") : lineBreak;

        private static string RandomCase(Random random, string value)
            => new(value.Select(c => random.Next(3) == 0 ? (char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)) : c).ToArray());

        [Fact]
        public void HeaderRules_MatchTheOriginalRegularExpressions()
        {
            var random = new Random(20261031);

            for (int i = 0; i < 20_000; i++)
            {
                var header = RandomHeader(random, out var lineBreak);
                var name = Names[random.Next(Names.Length)];
                var value = random.Next(2) == 0 ? "rewritten" : "10.0.0.1, 10.0.0.2";
                var context = $"header: {header.Replace("\r", "\\r").Replace("\n", "\\n\n")}\nname: {name}";

                Assert.True(ReferenceUpsert(header, name, value, lineBreak) == HttpUtility.UpsertHttpHostHeaderValue(header, name, value, lineBreak), "upsert\n" + context);
                Assert.True(ReferenceUpdate(header, name, value) == HttpUtility.UpdateHttpHostHeaderValue(header, name, value, lineBreak), "update\n" + context);
                Assert.True(ReferenceInsert(header, name, value, lineBreak) == HttpUtility.InsertHttpHostHeaderValue(header, name, value, lineBreak), "insert\n" + context);
                Assert.True(ReferenceDelete(header, name) == HttpUtility.DeleteHttpHostHeaderValue(header, name, lineBreak), "delete\n" + context);
            }
        }

        [Fact]
        public void BodyFraming_MatchesTheOriginalRegularExpressions()
        {
            var random = new Random(31102026);

            for (int i = 0; i < 20_000; i++)
            {
                var header = RandomHeader(random, out _);

                var reference = ContentLengthFinder.Match(header);
                long referenceLength = 0;
                bool referenceHas = reference.Success && long.TryParse(reference.Groups[1].Value, out referenceLength);

                bool has = HttpStreamParser.TryGetContentLength(header, out long length);

                //The original stopped at the first Content-Length line that was all digits; so does the new code.
                Assert.True(referenceHas == has && referenceLength == length, $"content-length\n{header.Replace("\r", "\\r").Replace("\n", "\\n\n")}");

                var encoding = TransferEncodingFinder.Match(header);
                bool referenceChunked = encoding.Success && encoding.Groups[1].Value.Contains("chunked", StringComparison.OrdinalIgnoreCase);
                bool chunked = HttpUtility.TryFindHeaderLine(header, "Transfer-Encoding", 0, out _, out int colon, out int end)
                    && header.AsSpan(colon + 1, end - colon - 1).Contains("chunked", StringComparison.OrdinalIgnoreCase);
                Assert.True(referenceChunked == chunked, "transfer-encoding\n" + header);
            }
        }
    }
}
