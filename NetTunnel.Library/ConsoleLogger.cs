using NetTunnel.Library.Interfaces;
using NTDLS.Helpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public class ConsoleLogger : ILogger
    {
        private readonly object _lock = new();
        private readonly NtLogSeverity _logLevel;
        private StreamWriter? _fileStream;

        public ConsoleLogger(NtLogSeverity logLevel)
        {
            _logLevel = logLevel;
        }

        public ConsoleLogger(NtLogSeverity logLevel, string logPath)
        {
            _logLevel = logLevel;

            if (string.IsNullOrWhiteSpace(logPath) == false)
            {
                Exceptions.Ignore(() => _fileStream = new StreamWriter(logPath));
            }
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

            DateTime dt = DateTime.Now;
            lock (_lock)
            {
                switch (severity)
                {
                    case NtLogSeverity.Debug:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case NtLogSeverity.Verbose:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case NtLogSeverity.Warning:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case NtLogSeverity.Exception:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }

                var logText = $"{severity} ({dt.ToShortDateString()} {dt.ToShortTimeString()}): {text}";

                Console.WriteLine(logText);
                _fileStream?.WriteLine(logText);

                Console.ResetColor();
            }
        }

        public void Dispose()
        {
            Exceptions.Ignore(() => _fileStream?.Close());
            Exceptions.Ignore(() => _fileStream?.Dispose());
        }
    }
}
