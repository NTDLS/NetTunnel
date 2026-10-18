using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LocalizerScan
{
    /// <summary>
    /// A batch of phrases sent to the translator in one request: how the request is written and how the reply is read.
    ///
    /// The request is JSON, <c>{"phrases":[{"id":0,"text":"Generate"}, ...]}</c>, and the reply is constrained to a JSON schema,
    /// <c>{"translations":[{"id":0,"text":"Erzeugen"}, ...]}</c>. Translations are matched to phrases by id, so line breaks, quotes and
    /// braces inside a phrase are never a problem, and a missing or repeated phrase is detected exactly.
    /// </summary>
    public static class TranslationBatch
    {
        /// <summary>
        /// The JSON schema the reply must follow (given to the translator as the response format).
        /// </summary>
        public const string ResponseSchema = """
            {
              "type": "object",
              "properties": {
                "translations": {
                  "type": "array",
                  "items": {
                    "type": "object",
                    "properties": {
                      "id":   { "type": "integer" },
                      "text": { "type": "string" }
                    },
                    "required": ["id", "text"],
                    "additionalProperties": false
                  }
                }
              },
              "required": ["translations"],
              "additionalProperties": false
            }
            """;

        /// <summary>
        /// The text of the request: every phrase with its id.
        /// </summary>
        public static string BuildInput(IReadOnlyList<string> batch)
        {
            var phrases = batch.Select((text, id) => new { id, text });

            return JsonSerializer.Serialize(new { phrases }, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        }

        public sealed class Result
        {
            /// <summary>
            /// False when the reply cannot be used (it is not the expected JSON, or a phrase is missing, repeated or unknown), and the whole batch should be asked again.
            /// </summary>
            public bool Understood { get; init; }

            /// <summary>
            /// The good translations by English phrase. A phrase whose translation was empty or invalid is left out, so it is tried again later.
            /// </summary>
            public Dictionary<string, string> Translations { get; } = [];

            /// <summary>
            /// What was wrong with the reply or with individual translations, for the log.
            /// </summary>
            public List<string> Messages { get; } = [];
        }

        private static Result NotUnderstood(string message)
        {
            var result = new Result { Understood = false };
            result.Messages.Add(message);
            return result;
        }

        /// <summary>
        /// Reads the reply to a batch.
        /// </summary>
        public static Result Parse(string reply, IReadOnlyList<string> batch, string languageCode)
        {
            Dictionary<int, string> byId;

            try
            {
                using var document = JsonDocument.Parse(Unwrap(reply));

                if (!document.RootElement.TryGetProperty("translations", out var translations) || translations.ValueKind != JsonValueKind.Array)
                {
                    return NotUnderstood("The reply has no \"translations\" list. Retrying..");
                }

                byId = [];
                foreach (var item in translations.EnumerateArray())
                {
                    if (!item.TryGetProperty("id", out var idElement) || idElement.ValueKind != JsonValueKind.Number || !idElement.TryGetInt32(out var id)
                        || !item.TryGetProperty("text", out var textElement) || textElement.ValueKind != JsonValueKind.String)
                    {
                        return NotUnderstood("A translation in the reply has no id or text. Retrying..");
                    }

                    if (!byId.TryAdd(id, textElement.GetString() ?? string.Empty))
                    {
                        return NotUnderstood($"The reply has phrase {id} more than once. Retrying..");
                    }
                }
            }
            catch (JsonException ex)
            {
                return NotUnderstood($"The reply is not valid JSON ({ex.Message}). Retrying..");
            }

            var unknown = byId.Keys.Where(id => id < 0 || id >= batch.Count).ToList();
            var missing = Enumerable.Range(0, batch.Count).Where(id => !byId.ContainsKey(id)).ToList();
            if (unknown.Count > 0 || missing.Count > 0)
            {
                return NotUnderstood($"The reply has {byId.Count:n0} translations for {batch.Count:n0} phrases"
                    + (missing.Count > 0 ? $" (missing {string.Join(", ", missing.Take(5))}{(missing.Count > 5 ? ", ..." : "")})" : "")
                    + ". Retrying..");
            }

            var result = new Result { Understood = true };

            for (int id = 0; id < batch.Count; id++)
            {
                var translation = byId[id].Trim();

                if (translation.Length == 0)
                {
                    result.Messages.Add($"The translation of \"{Show(batch[id])}\" is empty. Retrying..");
                    continue;
                }

                translation = MatchLineBreaks(batch[id], translation);

                var problems = TranslationValidator.GetProblems(languageCode, batch[id], translation);
                if (problems.Count > 0)
                {
                    result.Messages.Add($"The translation of \"{Show(batch[id])}\" is invalid ({string.Join("; ", problems)}). Retrying..");
                    continue;
                }

                result.Translations[batch[id]] = translation;
            }

            return result;
        }

        /// <summary>
        /// A reply should be bare JSON, but a model sometimes wraps it in a code fence or adds a sentence around it. Takes the JSON object out.
        /// </summary>
        private static string Unwrap(string reply)
        {
            var text = reply.Trim();
            if (text.StartsWith('{'))
            {
                return text;
            }

            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            return start >= 0 && end > start ? text.Substring(start, end - start + 1) : text;
        }

        /// <summary>
        /// Gives a translation the same kind of line breaks as its phrase: a translator may reply with "\n" to a phrase written with "\r\n".
        /// </summary>
        public static string MatchLineBreaks(string phrase, string translation)
        {
            if (phrase.Contains("\r\n"))
            {
                return Regex.Replace(translation, @"\r\n|\r|\n", "\r\n");
            }

            if (phrase.Contains('\n'))
            {
                return Regex.Replace(translation, @"\r\n|\r|\n", "\n");
            }

            return translation;
        }

        private static string Show(string phrase)
        {
            return phrase.Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }
}
