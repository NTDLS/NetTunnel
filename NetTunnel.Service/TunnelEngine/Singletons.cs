using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Logging;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.Logging;
using NTDLS.Persistence;

namespace NetTunnel.Service.TunnelEngine
{
    internal static class Singletons
    {
        private static ServiceEngine? _serviceEngine = null;
        public static ServiceEngine ServiceEngine
        {
            get
            {
                _serviceEngine ??= new ServiceEngine();
                return _serviceEngine;
            }
        }

        /// <summary>
        /// Logging provider: writes to the console (when interactive), a rolling log file (when configured)
        ///     and the Windows event log (warnings and exceptions) from a background thread.
        /// </summary>
        private static readonly object _loggerLock = new();
        private static QueuedLogger? _logger;
        private static RollingFileLogSink? _fileLogSink;
        private static EventLogSink? _eventLogSink;

        public static ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    lock (_loggerLock)
                    {
                        if (_logger == null)
                        {
                            var configuration = Configuration;
                            var sinks = new List<ILogSink>();

                            if (Environment.UserInteractive)
                            {
                                sinks.Add(new ConsoleLogSink());
                            }

                            _fileLogSink = new RollingFileLogSink(configuration.LogPath,
                                configuration.LogFileMaxSizeMB * 1024L * 1024L, configuration.LogFileRetainedCount);
                            sinks.Add(_fileLogSink);

                            if (OperatingSystem.IsWindows())
                            {
                                _eventLogSink = new EventLogSink { IsEnabled = configuration.LogToEventLog };
                                sinks.Add(_eventLogSink);
                            }

                            _logger = new QueuedLogger(configuration.LogLevel, sinks.ToArray());
                        }
                    }
                }
                return _logger;
            }
        }

        /// <summary>
        /// Writes out any queued log entries and closes the log sinks.
        /// </summary>
        public static void ShutdownLogger()
        {
            lock (_loggerLock)
            {
                _logger?.Dispose();
            }
        }

        /// <summary>
        /// Applies the logging settings from the configuration without requiring a service restart.
        /// </summary>
        private static void ApplyLoggingConfiguration(ServiceConfiguration configuration)
        {
            Logger.LogLevel = configuration.LogLevel;

            _fileLogSink?.Configure(configuration.LogPath,
                configuration.LogFileMaxSizeMB * 1024L * 1024L, configuration.LogFileRetainedCount);

            if (_eventLogSink != null)
            {
                _eventLogSink.IsEnabled = configuration.LogToEventLog;
            }
        }

        public static void UpdateConfiguration(ServiceConfiguration configuration)
        {
            //The service id identifies this instance (and which tunnels it owns), it must never change.
            configuration.ServiceId = Configuration.ServiceId;

            _configuration = configuration;
            CommonApplicationData.SaveToDisk(Constants.FriendlyName, _configuration);

            ApplyLoggingConfiguration(configuration);
        }

        private static ServiceConfiguration? _configuration = null;
        public static ServiceConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = CommonApplicationData.LoadFromDisk<ServiceConfiguration>(Constants.FriendlyName);

                    if (_configuration == null)
                    {
                        //We didn't find a config file, create one with default values.
                        _configuration = new ServiceConfiguration();
                        CommonApplicationData.SaveToDisk(Constants.FriendlyName, _configuration);
                    }

                    if (_configuration == null)
                    {
                        throw new Exception("Failed to load configuration.");
                    }
                }

                return _configuration;
            }
        }
    }
}
