using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Interfaces
{
    public interface ILogger : IDisposable
    {
        public delegate void OnLogDelegate(DateTime dateTime, NtLogSeverity severity, string message);
        public event OnLogDelegate? OnLog;

        public void Verbose(string message);
        public void Debug(string message);
        public void Warning(string message);
        public void Exception(string message);
        public void Exception(Exception ex);
        public void Exception(Exception ex, string message);
        public void Write(NtLogSeverity severity, string text);
    }
}
