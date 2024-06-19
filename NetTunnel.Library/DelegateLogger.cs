using NetTunnel.Library.Interfaces;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public class DelegateLogger : ILogger
    {
        private readonly NtLogSeverity _logLevel;

        public delegate void MessageWritten(NtLogSeverity severity, string message);

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

            _onMessageWritten(severity, text);
        }
        public void Dispose()
        {
        }
    }
}
