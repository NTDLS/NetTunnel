using NTDLS.Localization;
using System.Globalization;

namespace NetTunnel.UI.Forms
{
    /// <summary>
    /// A list of the languages to choose from.
    /// </summary>
    public sealed class FormLanguage : Form
    {
        private readonly ListBox _list = new();

        public FormLanguage(string? currentLanguage)
        {
            Text = Localize("Language");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = ShowInTaskbar = false;
            ClientSize = new Size(344, 400);

            var prompt = new Label { Text = Localize("Choose the language of the user interface."), Location = new Point(12, 12), Size = new Size(320, 34) };

            var choices = LanguageChoices.Build(Localizer.Default);
            _list.Location = new Point(12, 49);
            _list.Size = new Size(320, 300);
            _list.IntegralHeight = false;
            //Items rather than DataSource: a data source is not bound until the list is on a form, so the list would still be empty here.
            _list.Items.AddRange(choices.ToArray<object>());
            _list.SelectedIndex = LanguageChoices.IndexOf(choices, currentLanguage);
            _list.DoubleClick += (_, _) =>
            {
                if (_list.IndexFromPoint(_list.PointToClient(MousePosition)) >= 0)
                {
                    DialogResult = DialogResult.OK;
                }
            };

            var ok = new Button { Text = Localize("Ok"), DialogResult = DialogResult.OK, Location = new Point(148, 361), Size = new Size(88, 27) };
            var cancel = new Button { Text = Localize("Cancel"), DialogResult = DialogResult.Cancel, Location = new Point(244, 361), Size = new Size(88, 27) };

            AcceptButton = ok;
            CancelButton = cancel;
            Controls.AddRange([prompt, _list, ok, cancel]);
        }

        /// <summary>The chosen language: a culture code, or an empty string for Automatic.</summary>
        public string SelectedLanguage => (_list.SelectedItem as LanguageChoice)?.Code ?? string.Empty;
    }

    /// <summary>
    /// One language in the list the user chooses from.
    /// </summary>
    public sealed class LanguageChoice
    {
        /// <summary>The culture code to save ("de", "pt", "zh-Hans"), or an empty string for Automatic (follow Windows).</summary>
        public string Code { get; }
        public string DisplayName { get; }

        public LanguageChoice(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// The list of languages to choose from: "Automatic" (the Windows language), then every supported language, each shown in its
    /// own language so it can be found by someone who cannot read the current one ("Deutsch", "日本語").
    /// </summary>
    public static class LanguageChoices
    {
        public static List<LanguageChoice> Build(Localizer localizer)
        {
            var choices = new List<LanguageChoice>
            {
                new(string.Empty, localizer.Localize("Automatic: {0}", Capitalize(CultureInfo.CurrentUICulture.NativeName)))
            };

            var languages = new SupportedCultures().Collection
                .Select(o => new { o.Code, Native = Capitalize(o.Culture.NativeName), Name = localizer.Localize(o.Name) })
                .OrderBy(o => o.Native, StringComparer.CurrentCultureIgnoreCase);

            foreach (var language in languages)
            {
                var display = language.Name.Equals(language.Native, StringComparison.CurrentCultureIgnoreCase)
                    ? language.Native
                    : $"{language.Native} ({language.Name})";

                choices.Add(new LanguageChoice(language.Code, display));
            }

            return choices;
        }

        /// <summary>
        /// The position of a language setting in the list. A regional setting ("pt-BR") finds its language ("pt"); anything unknown finds Automatic.
        /// </summary>
        public static int IndexOf(IList<LanguageChoice> choices, string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(code.Trim(), predefinedOnly: true);
            }
            catch (CultureNotFoundException)
            {
                return 0;
            }

            for (var current = culture; !string.IsNullOrEmpty(current.Name); current = current.Parent)
            {
                for (int index = 1; index < choices.Count; index++)
                {
                    if (choices[index].Code.Equals(current.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }
                }
            }

            return 0;
        }

        private static string Capitalize(string name)
            => string.IsNullOrEmpty(name) ? name : char.ToUpper(name[0], CultureInfo.CurrentCulture) + name[1..];
    }

}
