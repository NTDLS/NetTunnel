using NetTunnel.Library;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI
{
    internal static class UIUtility
    {
        public static DelegateLogger CreateActiveWindowMessageBoxLogger(NtLogSeverity minimumSeverity)
        {
            return new DelegateLogger(minimumSeverity, (NtLogSeverity severity, string message) =>
            {
                var activeForm = Form.ActiveForm;
                activeForm ??= Application.OpenForms[0]; // If there is no active form, fall back to the "main form".
                activeForm?.InvokeMessageBox(message, FriendlyName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            });
        }
    }
}
