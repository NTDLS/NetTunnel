using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public class Logger : IDisposable
    {
        private readonly object _lock = new();
        private readonly LogLevel _logLevel;
        private StreamWriter? _fileStream;

        public enum LogLevel
        {
            Normal = 0,
            Debug = 1,
            Verbose = 2,
        }

        public Logger(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public Logger(LogLevel logLevel, string logPath)
        {
            _logLevel = logLevel;

            if (string.IsNullOrWhiteSpace(logPath) == false)
            {
                Utility.TryAndIgnore(() => _fileStream = new StreamWriter(logPath));
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
            if (severity == NtLogSeverity.Debug && _logLevel < LogLevel.Debug
                || severity == NtLogSeverity.Verbose && _logLevel < LogLevel.Verbose)
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
            Utility.TryAndIgnore(() => _fileStream?.Close());
            Utility.TryAndIgnore(() => _fileStream?.Dispose());
        }
    }
}
