using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Types
{
    public class VisualLogEntry
    {
        public DateTime DateTime { get; set; }
        public NtLogSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
