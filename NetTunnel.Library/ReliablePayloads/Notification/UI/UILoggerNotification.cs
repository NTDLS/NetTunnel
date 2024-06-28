using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.ReliablePayloads.Notification.UI
{
    public class UILoggerNotification : IRmNotification
    {
        public DateTime Timestamp { get; set; }
        public NtLogSeverity Severity { get; set; }
        public string Text { get; set; } = string.Empty;

        public UILoggerNotification()
        {
        }

        public UILoggerNotification(DateTime timestamp, NtLogSeverity severity, string text)
        {
            Timestamp = timestamp;
            Severity = severity;
            Text = text;
        }
    }
}
