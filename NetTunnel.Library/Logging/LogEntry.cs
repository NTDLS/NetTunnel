using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Logging
{
    /// <summary>
    /// A single log event, captured on the calling thread and written to the sinks by the logger's background thread.
    /// </summary>
    public class LogEntry
    {
        public DateTimeOffset Timestamp { get; init; }
        public NtLogSeverity Severity { get; init; }
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Full exception detail (type, message, stack trace and inner exceptions), if the entry was logged with an exception.
        /// </summary>
        public string? ExceptionDetail { get; init; }

        public int ThreadId { get; init; }

        /// <summary>
        /// True when the entry was logged by an OnLog handler. Such entries are written to the sinks but are not
        ///     raised through OnLog again; otherwise a handler that fails and logs it would loop forever.
        /// </summary>
        public bool SuppressOnLog { get; init; }

        /// <summary>
        /// e.g. "2026-10-18 14:03:27.512 -05:00 [Warning] (T:12) message"
        /// </summary>
        public string Format(bool includeExceptionDetail = true)
        {
            var text = $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Severity}] (T:{ThreadId}) {Message}";

            if (includeExceptionDetail && string.IsNullOrEmpty(ExceptionDetail) == false)
            {
                text += Environment.NewLine + ExceptionDetail;
            }

            return text;
        }
    }
}
