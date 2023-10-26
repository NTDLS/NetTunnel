using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    public class Logger
    {
        private readonly TunnelEngineCore _core;
        private readonly object _lock = new object();

        public Logger(TunnelEngineCore core)
        {
            _core = core;
        }

        public void Write(NtLogSeverity severity, string text)
        {
            if (severity == NtLogSeverity.Debug && _core.Configuration.DebugLogging == false)
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
                Console.WriteLine($"{severity} ({dt.ToShortDateString()} {dt.ToShortTimeString()}): {text}");
                Console.ResetColor();
            }
        }
    }
}
