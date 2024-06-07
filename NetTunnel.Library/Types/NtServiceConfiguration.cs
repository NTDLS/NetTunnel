namespace NetTunnel.Library.Types
{
    public class NtServiceConfiguration
    {
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
        /// The buffer size used by endpoint connections for sending and receiving data.
        /// </summary>
        public int EndpointBufferSize { get; set; } = 16384;

        /// <summary>
        /// Whether to log debug information to file.
        /// </summary>
        public bool DebugLogging { get; set; } = false;

        /// <summary>
        /// The duration in milliseconds to wait on message query operations.
        /// </summary>
        public int MessageQueryTimeoutMs { get; set; } = 60000;

        /// <summary>
        /// The delay in milliseconds between tunnel heartbeats.
        /// </summary>
        public int TunnelAndEndpointHeartbeatDelayMs { get; set; } = 10000;

        /// <summary>
        /// The number of 12-byte segments to generate for tunnel encryption.
        /// </summary>
        public int TunnelEncryptionKeySize { get; set; } = 8;

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
        public int InitialReceiveBufferSize { get; private set; } = 16 * 1024;

        /// <summary>
        ///The maximum size of the receive buffer. If the buffer ever gets full while receiving data it will be automatically resized up to MaxReceiveBufferSize.
        /// </summary>
        public int MaxReceiveBufferSize { get; set; } = 1024 * 1024;

        /// <summary>
        ///The growth rate of the auto-resizing for the receive buffer.
        /// </summary>
        public double ReceiveBufferGrowthRate { get; set; } = 0.2;

        #endregion
    }
}
