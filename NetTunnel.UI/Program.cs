using NetTunnel.UI.Forms;
using NTDLS.WinFormsHelpers;

namespace NetTunnel.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Theming.InitializeApplication();
            Application.Run(new FormMain());
        }
    }
}