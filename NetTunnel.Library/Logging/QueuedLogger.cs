using NetTunnel.Library.Interfaces;
using System.Diagnostics;
using System.Threading.Channels;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Logging
{
    /// <summary>
    /// Logger which captures entries on the calling thread and writes them to its sinks (and raises OnLog)
    ///     from a single background thread. Callers never block on file, console, event log or network I/O.
    /// </summary>
    public class QueuedLogger : ILogger
    {
        /// <summary>
        /// When this many entries are waiting to be written the oldest are discarded, so that a stalled sink
        ///     cannot consume unbounded memory. The number discarded is reported once the backlog clears.
        /// </summary>
        private const int MaxQueuedEntries = 50_000;

        /// <summary>
        /// Sinks are flushed at least this often while entries are arriving continuously.
        /// </summary>
        private const int MaxEntriesPerBatch = 1_000;

        private readonly Channel<LogEntry> _channel;
        private readonly Thread _writerThread;
        private readonly List<ILogSink> _sinks;
        private long _droppedCount;
        private volatile NtLogSeverity _logLevel;
        private bool _isDisposed;

        public event ILogger.OnLogDelegate? OnLog;

        public NtLogSeverity LogLevel
        {
            get => _logLevel;
            set => _logLevel = value;
        }

        public QueuedLogger(NtLogSeverity logLevel, params ILogSink[] sinks)
        {
            _logLevel = logLevel;
            _sinks = sinks.ToList();

            _channel = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(MaxQueuedEntries)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            }, (_) => Interlocked.Increment(ref _droppedCount));

            _writerThread = new Thread(WriterThreadProc)
            {
                IsBackground = true,
                Name = "LogWriterThread"
            };
            _writerThread.Start();
        }

        public void Verbose(string message) => Write(NtLogSeverity.Verbose, message);
        public void Debug(string message) => Write(NtLogSeverity.Debug, message);
        public void Warning(string message) => Write(NtLogSeverity.Warning, message);
        public void Exception(string message) => Write(NtLogSeverity.Exception, message);
        public void Exception(Exception ex) => Write(NtLogSeverity.Exception, ex.Message, ex);
        public void Exception(Exception ex, string message) => Write(NtLogSeverity.Exception, $"{message}: {ex.Message}", ex);
        public void Write(NtLogSeverity severity, string text) => Write(severity, text, null);

        public void Write(NtLogSeverity severity, string text, Exception? exception)
        {
            if (severity > _logLevel)
            {
                return;
            }

            var entry = new LogEntry
            {
                Timestamp = DateTimeOffset.Now,
                Severity = severity,
                Message = text,
                ExceptionDetail = exception?.ToString(), //Includes the type, stack trace and inner exceptions.
                ThreadId = Environment.CurrentManagedThreadId,
                SuppressOnLog = Environment.CurrentManagedThreadId == _writerThread.ManagedThreadId
            };

            if (_channel.Writer.TryWrite(entry) == false)
            {
                //Only fails once the logger has been disposed.
                Trace.WriteLine(entry.Format());
            }
        }

        private void WriterThreadProc()
        {
            var reader = _channel.Reader;

            while (reader.WaitToReadAsync().AsTask().GetAwaiter().GetResult())
            {
                int count = 0;
                while (count < MaxEntriesPerBatch && reader.TryRead(out var entry))
                {
                    Dispatch(entry);
                    count++;
                }

                var dropped = Interlocked.Exchange(ref _droppedCount, 0);
                if (dropped > 0)
                {
                    Dispatch(new LogEntry
                    {
                        Timestamp = DateTimeOffset.Now,
                        Severity = NtLogSeverity.Warning,
                        Message = $"The logger fell behind and discarded {dropped:n0} log entries.",
                        ThreadId = Environment.CurrentManagedThreadId,
                        SuppressOnLog = true
                    });
                }

                foreach (var sink in _sinks)
                {
                    try
                    {
                        sink.Flush();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"NetTunnel: Log sink {sink.GetType().Name} failed to flush: {ex}");
                    }
                }
            }
        }

        private void Dispatch(LogEntry entry)
        {
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Write(entry);
                }
                catch (Exception ex)
                {
                    //One broken sink must not stop the others.
                    Trace.TraceError($"NetTunnel: Log sink {sink.GetType().Name} failed: {ex}");
                }
            }

            if (entry.SuppressOnLog == false && OnLog != null)
            {
                try
                {
                    OnLog.Invoke(entry.Timestamp.LocalDateTime, entry.Severity, entry.Message);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"NetTunnel: Log subscriber failed: {ex}");
                }
            }
        }

        /// <summary>
        /// Stops accepting entries and waits (briefly) for the queue to be written out.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            _channel.Writer.TryComplete();
            _writerThread.Join(TimeSpan.FromSeconds(5));

            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Dispose();
                }
                catch
                {
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
