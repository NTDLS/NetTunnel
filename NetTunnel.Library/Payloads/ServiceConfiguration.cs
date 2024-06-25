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
        /// </summary>
        public string LogPath { get; set; } = string.Empty;

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
