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

                            if (OperatingSystem.IsWindows() && !Constants.IsUsingAlternateDataFolder)
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

            if (OperatingSystem.IsWindows() && _eventLogSink != null)
            {
                _eventLogSink.IsEnabled = configuration.LogToEventLog;
            }
        }

        /// <summary>
        /// Refuses settings which would stop the service from working. A bad value is saved to disk, so it would break every start
        /// afterwards too; a receive buffer of zero bytes, for example, reads as "the connection closed" on every read.
        /// </summary>
        internal static void ValidateConfiguration(ServiceConfiguration configuration)
        {
            static void Require(bool valid, string message)
            {
                if (!valid)
                {
                    throw new ArgumentException(message);
                }
            }

            Require(configuration.ServicePort is >= 1 and <= 65535, "The service port must be between 1 and 65535.");
            Require(configuration.InitialReceiveBufferSize >= 1024, "The initial receive buffer size must be at least 1,024 bytes.");
            Require(configuration.MaxReceiveBufferSize >= configuration.InitialReceiveBufferSize,
                "The maximum receive buffer size must not be smaller than the initial receive buffer size.");
            Require(configuration.ReceiveBufferGrowthRate > 0, "The receive buffer growth rate must be greater than zero.");
            Require(configuration.PingCadence >= 0, "The ping cadence cannot be negative.");
            Require(configuration.MessageQueryTimeoutMs >= 1000, "The message query timeout must be at least 1,000 milliseconds.");
            Require(configuration.EndpointHeartbeatDelayMs >= 100, "The endpoint heartbeat delay must be at least 100 milliseconds.");
            Require(configuration.StaleEndpointExpirationMs >= 0, "The stale endpoint expiration cannot be negative.");
            Require(configuration.TunnelCryptographyKeySize >= 128 && configuration.TunnelCryptographyKeySize % 128 == 0,
                "The tunnel cryptography key size must be a multiple of 128 bits.");
            Require(configuration.UdpSessionTimeoutMs >= 1000, "The UDP session timeout must be at least 1,000 milliseconds.");
            Require(configuration.UdpMaxSessionsPerEndpoint >= 1, "The maximum number of UDP sessions per endpoint must be at least 1.");
            Require(configuration.LogFileMaxSizeMB >= 1, "The log file size must be at least 1 MB.");
            Require(configuration.LogFileRetainedCount >= 0, "The number of retained log files cannot be negative.");
        }

        public static void UpdateConfiguration(ServiceConfiguration configuration)
        {
            ValidateConfiguration(configuration);

            //The service id identifies this instance (and which tunnels it owns), it must never change.
            configuration.ServiceId = Configuration.ServiceId;

            _configuration = configuration;
            CommonApplicationData.SaveToDisk(Constants.DataFolderName, _configuration);

            ApplyLoggingConfiguration(configuration);
        }

        private static ServiceConfiguration? _configuration = null;
        public static ServiceConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = CommonApplicationData.LoadFromDisk<ServiceConfiguration>(Constants.DataFolderName);

                    if (_configuration == null)
                    {
                        //We didn't find a config file, create one with default values.
                        _configuration = new ServiceConfiguration();
                        CommonApplicationData.SaveToDisk(Constants.DataFolderName, _configuration);
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
