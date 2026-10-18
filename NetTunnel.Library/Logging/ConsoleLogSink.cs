using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Logging
{
    /// <summary>
    /// Writes colorized log entries to the console. Only useful when running interactively.
    /// </summary>
    public class ConsoleLogSink : ILogSink
    {
        public void Write(LogEntry entry)
        {
            Console.ForegroundColor = entry.Severity switch
            {
                NtLogSeverity.Debug => ConsoleColor.DarkGray,
                NtLogSeverity.Verbose => ConsoleColor.Gray,
                NtLogSeverity.Warning => ConsoleColor.DarkYellow,
                NtLogSeverity.Exception => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };

            Console.WriteLine(entry.Format());
            Console.ResetColor();
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }
}
