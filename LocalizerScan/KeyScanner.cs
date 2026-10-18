using System.Text.RegularExpressions;

namespace LocalizerScan
{
    /// <summary>
    /// Finds the phrases the source code asks to have localized: the first argument of each <c>Localize("...")</c> call, and the
    /// wording in the Windows Forms designer files (see <see cref="ExtractDesignerKeys"/>).
    /// That covers the static helper (<c>Localize("Generate")</c>), formatted text (<c>Localize("Copied {0} files", count)</c>)
    /// and calls through a localizer (<c>Localizer.Default.Localize("...")</c>).
    /// </summary>
    public static partial class KeyScanner
    {
        //A normal string: "text", with \" and other escapes allowed inside. The call can be split across lines.
        [GeneratedRegex("""\bLocalize\(\s*"((?:[^"\\]|\\.)*)"\s*[,)]""", RegexOptions.Singleline)]
        private static partial Regex RegularString();

        //A verbatim string: @"text", where "" is a quote.
        [GeneratedRegex("""\bLocalize\(\s*@"((?:[^"]|"")*)"\s*[,)]""", RegexOptions.Singleline)]
        private static partial Regex VerbatimString();

        //Designer code: a field per control ("private System.Windows.Forms.Button buttonOk;"), and then its text ("buttonOk.Text = "OK";").
        [GeneratedRegex("""private\s+(?:[\w.]+\.)?(\w+)\s+(\w+)\s*;""")]
        private static partial Regex ControlField();

        [GeneratedRegex("""^\s*(?:(?:this\.)?(\w+)\.)?(Text|PlaceholderText)\s*=\s*"((?:[^"\\]|\\.)*)"\s*;""", RegexOptions.Multiline)]
        private static partial Regex TextAssignment();

        /// <summary>
        /// The controls whose designer text is wording, matching what FormLocalizer translates when a form is created. (The text of a
        /// text box or a combo box is not wording: it is what the user types or the program fills in.)
        /// </summary>
        private static readonly HashSet<string> _textControls = new(StringComparer.Ordinal)
        {
            "Label", "LinkLabel", "Button", "CheckBox", "RadioButton", "GroupBox", "TabPage", "ColumnHeader",
            "ToolStripMenuItem", "ToolStripLabel", "ToolStripButton", "ToolStripStatusLabel", "ToolStripDropDownButton", "ToolStripSplitButton"
        };

        /// <summary>
        /// The wording the Windows Forms designer put on a form, in the source of a designer file. A form's own text is
        /// the "Text = ..." with no control in front of it.
        /// </summary>
        public static IEnumerable<string> ExtractDesignerKeys(string designerCode)
        {
            var typeByControl = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (Match field in ControlField().Matches(designerCode))
            {
                typeByControl[field.Groups[2].Value] = field.Groups[1].Value;
            }

            foreach (Match assignment in TextAssignment().Matches(designerCode))
            {
                var control = assignment.Groups[1].Value;
                var property = assignment.Groups[2].Value;
                var text = Regex.Unescape(assignment.Groups[3].Value);

                bool wording;
                if (control.Length == 0)
                {
                    wording = property == "Text"; //The form itself.
                }
                else if (typeByControl.TryGetValue(control, out var type))
                {
                    wording = property == "PlaceholderText" ? type == "TextBox" : _textControls.Contains(type);
                }
                else
                {
                    wording = false;
                }

                if (wording && NTDLS.Localization.Phrases.IsTranslatable(text))
                {
                    yield return text;
                }
            }
        }

        /// <summary>
        /// The folders that are never scanned: build output, tools, tests, including projects named *.Tests (their phrases are made up) and reference copies.
        /// </summary>
        private static readonly string[] _ignoredFolders = ["bin", "obj", ".git", ".vs", "packages", "Tests", "LocalizerScan", "Translations", "publish"];

        /// <summary>
        /// The source code with its comments blanked out, so that a Localize("...") that is only an example in a comment (or has
        /// been commented out) is not taken for a phrase. Text inside string literals is left alone ("http://..." is not a comment).
        /// Line breaks are kept.
        /// </summary>
        public static string StripComments(string code)
        {
            var result = new System.Text.StringBuilder(code.Length);

            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];

                if (c == '/' && i + 1 < code.Length && code[i + 1] == '/')
                {
                    while (i < code.Length && code[i] != '\n')
                    {
                        i++;
                    }
                    i--; //Keep the line break.
                }
                else if (c == '/' && i + 1 < code.Length && code[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < code.Length && !(code[i] == '*' && code[i + 1] == '/'))
                    {
                        if (code[i] == '\n')
                        {
                            result.Append('\n');
                        }
                        i++;
                    }
                    i++; //Past the "*/".
                }
                else if (c == '"')
                {
                    //A string literal: copy it whole. A verbatim string (@"...") ends at a quote that is not doubled,
                    //  any other at a quote that is not escaped. Raw strings (""") are not used in this code base.
                    bool verbatim = i > 0 && (code[i - 1] == '@' || (i > 1 && code[i - 2] == '@' && code[i - 1] == '$'));
                    result.Append(c);
                    i++;
                    while (i < code.Length)
                    {
                        result.Append(code[i]);
                        if (verbatim)
                        {
                            if (code[i] == '"')
                            {
                                if (i + 1 < code.Length && code[i + 1] == '"')
                                {
                                    result.Append('"');
                                    i += 2;
                                    continue;
                                }
                                break;
                            }
                        }
                        else if (code[i] == '\\' && i + 1 < code.Length)
                        {
                            result.Append(code[i + 1]);
                            i += 2;
                            continue;
                        }
                        else if (code[i] == '"')
                        {
                            break;
                        }
                        i++;
                    }
                }
                else if (c == '\'')
                {
                    //A character literal such as '"' or '\''.
                    result.Append(c);
                    i++;
                    while (i < code.Length)
                    {
                        result.Append(code[i]);
                        if (code[i] == '\\' && i + 1 < code.Length)
                        {
                            result.Append(code[i + 1]);
                            i += 2;
                            continue;
                        }
                        if (code[i] == '\'')
                        {
                            break;
                        }
                        i++;
                    }
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// The phrases asked for in a piece of source code.
        /// </summary>
        public static IEnumerable<string> ExtractKeys(string sourceCode)
        {
            sourceCode = StripComments(sourceCode);

            foreach (Match match in RegularString().Matches(sourceCode))
            {
                yield return Regex.Unescape(match.Groups[1].Value);
            }

            foreach (Match match in VerbatimString().Matches(sourceCode))
            {
                yield return match.Groups[1].Value.Replace("\"\"", "\"");
            }
        }

        /// <summary>
        /// The source files under a folder that should be scanned.
        /// </summary>
        public static IEnumerable<string> FindSourceFiles(string rootPath)
        {
            var root = Path.GetFullPath(rootPath);

            return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(file => !Path.GetRelativePath(root, file)
                    .Split(Path.DirectorySeparatorChar)
                    .SkipLast(1) //The file name itself is not a folder.
                    .Any(folder => _ignoredFolders.Contains(folder, StringComparer.OrdinalIgnoreCase) || folder.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// All of the distinct phrases (ignoring case, as the database does) asked for by the source files under a folder.
        /// </summary>
        public static HashSet<string> ScanFolder(string rootPath)
        {
            var keys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var file in FindSourceFiles(rootPath))
            {
                var code = File.ReadAllText(file);

                foreach (var key in ExtractKeys(code))
                {
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        keys.Add(key);
                    }
                }

                if (file.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var key in ExtractDesignerKeys(code))
                    {
                        keys.Add(key);
                    }
                }
            }

            return keys;
        }
    }
}
