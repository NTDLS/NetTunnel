﻿using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class Logger
    {
        private readonly TunnelEngineCore _core;
        private readonly object _lock = new();

        public Logger(TunnelEngineCore core)
        {
            _core = core;
        }

        public void Write(NtLogSeverity severity, string text)
        {
            if (severity == NtLogSeverity.Debug && Singletons.Configuration.DebugLogging == false)
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
