using NetTunnel.Library.Interfaces;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public class DelegateLogger : ILogger
    {
        public delegate void MessageWritten(NtLogSeverity severity, string message);
        public event ILogger.OnLogDelegate? OnLog;

        private readonly NtLogSeverity _logLevel;
        private readonly MessageWritten _onMessageWritten;

        public DelegateLogger(NtLogSeverity logLevel, MessageWritten onMessageWritten)
        {
            _logLevel = logLevel;
            _onMessageWritten = onMessageWritten;
        }

        public void Verbose(string message) => Write(NtLogSeverity.Verbose, message);
        public void Debug(string message) => Write(NtLogSeverity.Debug, message);
        public void Warning(string message) => Write(NtLogSeverity.Warning, message);
        public void Exception(string message) => Write(NtLogSeverity.Exception, message);
        public void Exception(Exception ex) => Write(NtLogSeverity.Exception, ex.Message);
        public void Exception(Exception ex, string message) => Write(NtLogSeverity.Exception, $"{message}: {ex.Message}");

        public void Write(NtLogSeverity severity, string text)
        {
            if (severity > _logLevel)
            {
                return;
            }

            OnLog?.Invoke(DateTime.Now, severity, text);

            _onMessageWritten(severity, text);
        }
        public void Dispose()
        {
        }
    }
}
