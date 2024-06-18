using NetTunnel.Library;
using System.Diagnostics;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class Logger
    {
        private readonly ServiceEngine _serviceEngine;
        private readonly object _lock = new();

        public Logger(ServiceEngine serviceEngine)
        {
            _serviceEngine = serviceEngine;
        }

        public void Write(Exception ex)
            => Write(NtLogSeverity.Exception, ex.Message);

        public void Write(Exception ex, string message)
            => Write(NtLogSeverity.Exception, $"{message}: {ex.Message}");

        public void Write(NtLogSeverity severity, string text)
        {
            if (severity == NtLogSeverity.Debug && Singletons.Configuration.DebugLogging == false
                || severity == NtLogSeverity.Verbose && Singletons.Configuration.VerboseLogging == false)
            {
                return;
            }

            DateTime dt = DateTime.Now;
            lock (_lock)
            {
                EventLogEntryType eventLogType = EventLogEntryType.Information;

                switch (severity)
                {
                    case NtLogSeverity.Debug:
                        eventLogType = EventLogEntryType.Information;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case NtLogSeverity.Verbose:
                        eventLogType = EventLogEntryType.Information;
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case NtLogSeverity.Warning:
                        eventLogType = EventLogEntryType.Warning;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case NtLogSeverity.Exception:
                        eventLogType = EventLogEntryType.Error;
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }

                Console.WriteLine($"{severity} ({dt.ToShortDateString()} {dt.ToShortTimeString()}): {text}");
                Console.ResetColor();
                Utility.TryAndIgnore(() => EventLog.WriteEntry(Constants.EventSourceName, text, eventLogType));
            }
        }
    }
}
