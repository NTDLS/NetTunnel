using Dapper;
using Microsoft.Data.Sqlite;
using NTDLS.EmbeddedResource;
using NTDLS.SqliteDapperWrapper;
using NTDLS.Localization;
using System.Text;
using System.Text.RegularExpressions;
using VPT.Einkr.Client;

namespace LocalizerScan
{
    internal class Program
    {
        private static readonly SupportedCultures _supportedCultures = new SupportedCultures();

        private const string ModelName = "Stratum-27B";
        private const string EscalationModelName = "";
        private const int DefaultConcurrency = 4;
        private const int BatchSize = 25;
        private const int MaxBatchAttempts = 5;

        private static readonly Lock _consoleLock = new();
        private static readonly Lock _databaseLock = new(); //SQLite allows only one writer at a time.

        private static async Task<int> Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: LocalizerScan <rootPath> <projectName> [concurrency]");
                return 1;
            }

            var apiKey = File.ReadAllText("C:\\EinkrKey.txt").Trim();

            //The client is stateless and safe to share, so all of the concurrent translations use one connection pool.
            using var einkr = new EinkrAIClient(ModelName, apiKey);

            var escalationModelName = Environment.GetEnvironmentVariable("LOCALIZER_ESCALATION_MODEL");
            if (string.IsNullOrWhiteSpace(escalationModelName))
            {
                escalationModelName = EscalationModelName;
            }

            using var escalationEinkr = string.IsNullOrWhiteSpace(escalationModelName) ? null : new EinkrAIClient(escalationModelName, apiKey);
            if (escalationEinkr == null)
            {
                Console.WriteLine("No escalation model is configured, failed translations will only be retried with the primary model.");
            }

            var rootPath = args[0];
            var translationDatabase = Path.Join(rootPath, args[1], "Translations", "Translations.db");
            var concurrency = args.Length > 2 && int.TryParse(args[2], out var requested) && requested > 0 ? requested : DefaultConcurrency;

            EnsureDatabase(translationDatabase);
            ScanSourceFilesAndAddMissingKeys(rootPath, translationDatabase);
            ClearInvalidTranslations(translationDatabase);
            await FillInMissingTranslations(translationDatabase, einkr, escalationEinkr, "English", concurrency);

            return 0;
        }

        /// <summary>
        /// Makes sure the translation database exists and has the current schema: it is created if it is missing, and an older one
        /// is brought up to date. Each language is a column named by its culture code ("de", "zh-Hans"); a column that still has the
        /// older language name ("German", "Chinese simplified") is renamed, and a language with no column yet gets one.
        /// Every row records when it was last changed (UTC), kept up to date by triggers so that edits made in any tool count.
        /// </summary>
        private static void EnsureDatabase(string translationDatabase)
        {
            if (!File.Exists(translationDatabase))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(translationDatabase)!);

                var languages = string.Concat(_supportedCultures.Collection.Where(o => o.Code != "en").Select(o => $"\"{o.Code}\" TEXT, "));

                using var created = new SqliteManagedInstance(translationDatabase);
                created.Execute($"CREATE TABLE IF NOT EXISTS \"Translation\" (\"TranslationId\" INTEGER PRIMARY KEY AUTOINCREMENT, \"Phrase\" TEXT NOT NULL UNIQUE COLLATE NOCASE, {languages}\"LastModified\" TEXT)");
                Console.WriteLine($"Created the translation database: {translationDatabase}");
            }

            using var translationDb = new SqliteManagedInstance(translationDatabase);

            //The key column cannot be called "Id": column names are not case sensitive, and "id" is the code for Indonesian.
            var keyColumn = translationDb.Query<string>("SELECT name FROM pragma_table_info('Translation') WHERE pk = 1").FirstOrDefault();
            if (keyColumn != null && keyColumn.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                translationDb.Execute("ALTER TABLE Translation RENAME COLUMN \"Id\" TO \"TranslationId\"");
                Console.WriteLine("Renamed the key column \"Id\" to \"TranslationId\".");
            }

            //The key column holds the English phrase. It used to be called English.
            var phraseColumns = translationDb.Query<string>("SELECT name FROM pragma_table_info('Translation')").ToList();
            if (phraseColumns.Contains("English", StringComparer.OrdinalIgnoreCase) && !phraseColumns.Contains("Phrase", StringComparer.OrdinalIgnoreCase))
            {
                translationDb.Execute("ALTER TABLE Translation RENAME COLUMN \"English\" TO \"Phrase\"");
                Console.WriteLine("Renamed the column \"English\" to \"Phrase\".");
            }

            var columns = new HashSet<string>(translationDb.Query<string>("SELECT name FROM pragma_table_info('Translation')"), StringComparer.OrdinalIgnoreCase);

            foreach (var culture in _supportedCultures.Collection.Where(o => o.Code != "en"))
            {
                if (columns.Contains(culture.Code))
                {
                    continue;
                }

                if (columns.Contains(culture.Name))
                {
                    translationDb.Execute($"ALTER TABLE Translation RENAME COLUMN \"{culture.Name}\" TO \"{culture.Code}\"");
                    Console.WriteLine($"Renamed the column \"{culture.Name}\" to \"{culture.Code}\".");
                }
                else
                {
                    translationDb.Execute($"ALTER TABLE Translation ADD COLUMN \"{culture.Code}\" TEXT");
                    Console.WriteLine($"Added the column \"{culture.Code}\" ({culture.Name}).");
                }
            }

            //When a row was last changed (UTC, "yyyy-MM-dd HH:mm:ss"). Rows from before this column get the time of this run.
            if (!columns.Contains("LastModified"))
            {
                translationDb.Execute("ALTER TABLE Translation ADD COLUMN \"LastModified\" TEXT");
                Console.WriteLine("Added the column \"LastModified\".");
            }
            translationDb.Execute("UPDATE Translation SET LastModified = strftime('%Y-%m-%d %H:%M:%S', 'now') WHERE LastModified IS NULL");

            //The triggers keep it current for any change, from this tool or any other: a changed row gets the time unless the change set it itself.
            translationDb.Execute("""
                CREATE TRIGGER IF NOT EXISTS Translation_SetLastModified AFTER INSERT ON Translation
                FOR EACH ROW WHEN NEW.LastModified IS NULL
                BEGIN
                    UPDATE Translation SET LastModified = strftime('%Y-%m-%d %H:%M:%S', 'now') WHERE TranslationId = NEW.TranslationId;
                END
                """);
            translationDb.Execute("""
                CREATE TRIGGER IF NOT EXISTS Translation_UpdateLastModified AFTER UPDATE ON Translation
                FOR EACH ROW WHEN NEW.LastModified IS OLD.LastModified
                BEGIN
                    UPDATE Translation SET LastModified = strftime('%Y-%m-%d %H:%M:%S', 'now') WHERE TranslationId = NEW.TranslationId;
                END
                """);
        }

        /// <summary>
        /// Writes a line prefixed with the language it relates to, since several languages are translated at once.
        /// </summary>
        private static void Log(string language, string message)
        {
            lock (_consoleLock)
            {
                Console.WriteLine($"[{language}] {message}");
            }
        }

        private static void ScanSourceFilesAndAddMissingKeys(string rootPath, string translationDatabase)
        {
            try
            {
                Console.WriteLine($"Scanning: {rootPath}");
                if (!Directory.Exists(rootPath))
                {
                    Console.Error.WriteLine("Root path does not exist.");
                    return;
                }

                if (!File.Exists(translationDatabase))
                {
                    Console.Error.WriteLine($"The translation database was not found: {translationDatabase}");
                    return;
                }

                //The phrases the source code asks for with Localize("...").
                var keysToTranslate = KeyScanner.ScanFolder(rootPath);

                //The language names are shown to the user (to choose a language) so they are phrases too.
                foreach (var culture in _supportedCultures.Collection)
                {
                    keysToTranslate.Add(culture.Name);
                }

                Console.WriteLine($"Found {keysToTranslate.Count} unique localization keys.");

                //The English phrase is unique (case insensitive), so phrases that are already there are left untouched.
                using var translationDb = new SqliteManagedInstance(translationDatabase);
                using var transaction = translationDb.BeginTransaction();

                int added = 0;
                foreach (var key in keysToTranslate)
                {
                    if (translationDb.ExecuteScalar<int>("SELECT COUNT(1) FROM Translation WHERE Phrase = @key", new { key }) == 0)
                    {
                        translationDb.Execute("INSERT INTO Translation (Phrase) VALUES (@key)", new { key });
                        Console.WriteLine($"Added \"{key}\" to the translation database.");
                        added++;
                    }
                }

                transaction.Commit();
                Console.WriteLine($"Added {added:n0} new phrases.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        /// <summary>
        /// Clears any existing translations that fail validation, so that they are translated again.
        /// </summary>
        private static void ClearInvalidTranslations(string translationDatabase)
        {
            int cleared = 0;

            using var translationDb = new SqliteManagedInstance(translationDatabase);

            foreach (var culture in _supportedCultures.Collection.Where(o => o.Code != "en"))
            {
                var column = $"\"{culture.Code}\"";

                var translations = translationDb.Query<(string Phrase, string Value)>(
                    $"SELECT Phrase, {column} as Value FROM Translation WHERE {column} IS NOT NULL AND {column} != ''").ToList();

                using var transaction = translationDb.BeginTransaction();

                foreach (var (key, value) in translations)
                {
                    var problems = TranslationValidator.GetProblems(culture.Code, key, value);
                    if (problems.Count > 0)
                    {
                        Log(culture.Name, $"Cleared \"{key}\" = \"{value}\": {string.Join("; ", problems)}");
                        translationDb.Execute($"UPDATE Translation SET {column} = NULL WHERE Phrase = @key", new { key });
                        cleared++;
                    }
                }

                transaction.Commit();
            }

            Console.WriteLine($"Cleared {cleared:n0} invalid translations.");
        }

        /// <summary>
        /// Translates the languages concurrently. Each language is its own column in the translation table,
        ///  so the only things shared between them are the (thread-safe) clients and the database lock.
        /// </summary>
        private static async Task FillInMissingTranslations(string translationDatabase, EinkrAIClient chat, EinkrAIClient? escalationChat, string sourceLanguage, int concurrency)
        {
            var cultures = _supportedCultures.Collection.Where(o => o.Code != "en").ToList();
            var promptTemplate = EmbeddedResourceReader.LoadText(@"EmbeddedText\SystemPrompt.txt");

            Console.WriteLine($"Translating {cultures.Count:n0} languages, {concurrency} at a time.");

            await Parallel.ForEachAsync(cultures, new ParallelOptions { MaxDegreeOfParallelism = concurrency },
                async (culture, cancellationToken) =>
                {
                    try
                    {
                        await TranslateLanguage(translationDatabase, culture, chat, escalationChat, sourceLanguage, promptTemplate, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        //One failing language should not stop the others.
                        Log(culture.Name, $"Failed: {ex.Message}");
                    }
                });
        }

        private static async Task TranslateLanguage(string translationDatabase, CultureInfoSettings targetLanguage, EinkrAIClient chat, EinkrAIClient? escalationChat,
            string sourceLanguage, string promptTemplate, CancellationToken cancellationToken)
        {
            var languageName = targetLanguage.Name;
            var column = $"\"{targetLanguage.Code}\"";

            //Only the phrases that have no translation need to be translated.
            List<string> missing;
            lock (_databaseLock)
            {
                using var translationDb = new SqliteManagedInstance(translationDatabase);
                missing = translationDb.Query<string>($"SELECT Phrase FROM Translation WHERE {column} IS NULL OR {column} = ''").ToList();
            }

            if (missing.Count == 0)
            {
                return; //No phrases to translate.
            }

            var phrases = missing.ToDictionary(o => o, o => (string?)null);

            Log(languageName, $"{phrases.Count:n0} elements -> {languageName}");

            var promptText = promptTemplate
                .Replace("{sourceLanguage}", sourceLanguage)
                .Replace("{targetLanguage}", languageName);

            //Phrases whose batch keeps failing are left empty, so that the next run tries them again.
            var skipped = new HashSet<string>();

            while (phrases.Any(o => o.Value == null && !skipped.Contains(o.Key)))
            {
                var batch = phrases.Where(o => o.Value == null && !skipped.Contains(o.Key)).Take(BatchSize).Select(o => o.Key).ToList();

                var translations = new Dictionary<string, string>();
                var remaining = batch;

                //Only the phrases that failed validation are retried: first with the primary model, then whatever is
                //  still failing is handed to the larger model. A phrase that one model cannot translate must not
                //  hold up the others, nor keep being retried in ever smaller batches with the same model.
                var models = new List<(EinkrAIClient Client, string Stage)> { (chat, "Processing") };
                if (escalationChat != null)
                {
                    models.Add((escalationChat, "Escalating"));
                }

                foreach (var (client, stage) in models)
                {
                    for (int attempt = 1; attempt <= MaxBatchAttempts && remaining.Count > 0; attempt++)
                    {
                        Log(languageName, $"{stage} batch of {remaining.Count:n0} elements -> {languageName}{(stage == "Escalating" ? " with the larger model" : "")}{(attempt > 1 ? $" (attempt {attempt})" : "")}");

                        var translated = await TranslateBatch(languageName, targetLanguage.Code, client, promptText, remaining, cancellationToken);
                        if (translated == null)
                        {
                            continue; //The response could not be parsed at all.
                        }

                        foreach (var (key, translation) in translated)
                        {
                            translations[key] = translation;
                        }

                        remaining = remaining.Where(o => !translated.ContainsKey(o)).ToList();
                    }
                }

                if (remaining.Count > 0)
                {
                    Log(languageName, $"Giving up on {remaining.Count:n0} elements after {MaxBatchAttempts} attempts{(escalationChat != null ? " with each model" : "")}, they will be retried on the next run.");
                    skipped.UnionWith(remaining);
                }

                if (translations.Count == 0)
                {
                    continue;
                }

                //Save the translated phrases from this batch right away so that they are not lost if a later batch fails.
                lock (_databaseLock)
                {
                    using var translationDb = new SqliteManagedInstance(translationDatabase);
                    using var transaction = translationDb.BeginTransaction();

                    foreach (var (key, translation) in translations)
                    {
                        phrases[key] = translation;
                        translationDb.Execute($"UPDATE Translation SET {column} = @translation WHERE Phrase = @key", new { key, translation });
                    }

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Translates one batch of phrases, returning null if the response could not be parsed. Phrases whose translation
        /// is empty or fails validation are left out of the result (and are retried later), so they do not discard the good ones.
        /// </summary>
        private static async Task<Dictionary<string, string>?> TranslateBatch(string languageName, string languageCode, EinkrAIClient chat, string promptText,
            List<string> batch, CancellationToken cancellationToken)
        {
            //The reply is constrained to a JSON schema (see TranslationBatch), so it is JSON by contract and its translations are matched by id.
            ChatCompletion response = await chat.CompleteChatAsync([
                    new SystemChatMessage(promptText),
                    new UserChatMessage(TranslationBatch.BuildInput(batch))
                ],
                responseFormat: ChatResponseFormat.CreateJsonSchemaFormat("translations", TranslationBatch.ResponseSchema),
                cancellationToken: cancellationToken);

            if (response.FinishReason == "length")
            {
                //The reply hit the output limit and the JSON is cut off.
                Log(languageName, "The reply was cut off before the end. Retrying..");
                return null;
            }

            var result = TranslationBatch.Parse(response.Content[0].Text, batch, languageCode);

            foreach (var message in result.Messages)
            {
                Log(languageName, message);
            }

            return result.Understood ? result.Translations : null;
        }
    }
}
