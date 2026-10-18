using NetTunnel.Library;
using NTDLS.WinFormsHelpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI
{
    internal static class UIUtility
    {
        /// <summary>
        /// Adds an item to the menu. The tag is the language-independent key which is used to identify the item when it is clicked.
        /// </summary>
        public static ToolStripItem AddMenuItem(ContextMenuStrip menu, string key, string text)
        {
            var item = menu.Items.Add(text);
            item.Tag = key;
            return item;
        }

        public static string DirectionText(NtDirection direction) => direction switch
        {
            NtDirection.Inbound => Localize("Inbound"),
            NtDirection.Outbound => Localize("Outbound"),
            _ => Localize("Undefined")
        };

        public static string StatusText(NtTunnelStatus status) => status switch
        {
            NtTunnelStatus.Connecting => Localize("Connecting"),
            NtTunnelStatus.Established => Localize("Established"),
            NtTunnelStatus.Disconnected => Localize("Disconnected"),
            NtTunnelStatus.Stopped => Localize("Stopped"),
            _ => Localize("Undefined")
        };

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
