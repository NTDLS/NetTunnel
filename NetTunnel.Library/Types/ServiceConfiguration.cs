using static NetTunnel.Library.Logger;

namespace NetTunnel.Library.Types
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
        public int ManagementPort { get; set; } = 52845;
        /// <summary>
        /// Whether the management web-services should use SSL or not.
        /// </summary>
        public bool ManagementPortUseSSL { get; set; } = true;
        /// <summary>
        /// The key size to use when generating the self signed SSL certificate.
        /// </summary>
        public int ManagementPortRSASize { get; set; } = 2048;

        /// <summary>
        /// Level of information to log to the file/console/etc.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Verbose;

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
        public int TunnelAndEndpointHeartbeatDelayMs { get; set; } = 5000;

        /// <summary>
        /// The number of 12-byte segments to generate for tunnel cryptography.
        /// </summary>
        public int TunnelCryptographyKeySize { get; set; } = 16;

        /// <summary>
        /// The maximum number of milliseconds to allow an endpoint to remain connected without read/write activity.
        /// </summary>
        public int StaleEndpointExpirationMs { get; set; } = 600000;


        #region Reliable Messaging Configuration.

        /// <summary>
        /// The frame header delimiter. Used to literally seperate and detect the beginning of each packet.
        /// </summary>
        public int FrameDelimiter = 1277337552;

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
