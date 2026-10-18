using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    public class ServiceConfiguration
    {
        /// <summary>
        /// The id for this instance of the tunnel service.
        /// </summary>
        public Guid ServiceId { get; set; } = Guid.NewGuid();
        /// <summary>
        /// The HTTP/HTTPS port for the management web-services.
        /// </summary>
        public int ServicePort { get; set; } = 52845;

        /// <summary>
        /// When true, endpoints listen on IPv6 as well as IPv4 (one dual-stack listener), and can connect to IPv6 destinations.
        /// When false, only IPv4 is used.
        /// </summary>
        public bool EnableIPv6 { get; set; } = true;

        /// <summary>
        /// The number of milliseconds to wait between pings to the remote service. (0 = disabled);
        /// </summary>
        public int PingCadence { get; set; } = 5000;

        /// <summary>
        /// Level of information to log to the file/console/etc.
        /// </summary>
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtLogSeverity LogLevel { get; set; } = NtLogSeverity.Verbose;

        /// <summary>
        /// The file path to log to. If the string is empty, the log will not be written to file.
        /// Relative paths are relative to the service's installation directory.
        /// </summary>
        public string LogPath { get; set; } = string.Empty;

        /// <summary>
        /// The log file is rolled over (NetTunnel.log -> NetTunnel.1.log, etc.) once it reaches this size.
        /// </summary>
        public int LogFileMaxSizeMB { get; set; } = 10;

        /// <summary>
        /// The number of rolled-over log files to keep in addition to the current one.
        /// </summary>
        public int LogFileRetainedCount { get; set; } = 5;

        /// <summary>
        /// Whether warnings and exceptions are also written to the Windows Application event log.
        /// </summary>
        public bool LogToEventLog { get; set; } = true;

        /// <summary>
        /// The duration in milliseconds to wait on message query operations.
        /// </summary>
        public int MessageQueryTimeoutMs { get; set; } = 10000;

        /// <summary>
        /// The delay in milliseconds between tunnel heartbeats.
        /// </summary>
        public int EndpointHeartbeatDelayMs { get; set; } = 5000;

        /// <summary>
        /// The number bits to generate for tunnel cryptography.
        /// </summary>
        public int TunnelCryptographyKeySize { get; set; } = 4096;

        /// <summary>
        /// The maximum number of milliseconds to allow an endpoint to remain connected without read/write activity.
        /// </summary>
        public int StaleEndpointExpirationMs { get; set; } = 0;

        #region Reliable Messaging Configuration.

        /// <summary>
        /// The initial size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize.
        /// </summary>
        public int InitialReceiveBufferSize { get; set; } = 16 * 1024;

        /// <summary>
        ///The maximum size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize.
        /// </summary>
        public int MaxReceiveBufferSize { get; set; } = 1024 * 1024;

        /// <summary>
        ///The growth rate for auto-resizing the receive buffer from its initial size to its maximum size..
        /// </summary>
        public double ReceiveBufferGrowthRate { get; set; } = 0.2;

        #endregion
    }
}
