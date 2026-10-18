using NetTunnel.UI.Forms;
using NTDLS.Localization;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // The language is the user's choice in the settings, or else the Windows language.
            Localizer.Default.SetLanguage(UISettings.Load().Language);

            ApplicationConfiguration.Initialize();
            Theming.InitializeApplication();
            Application.Run(new FormMain());
        }
    }
}