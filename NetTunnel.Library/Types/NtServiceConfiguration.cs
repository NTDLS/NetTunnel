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
        public bool DebugLogging { get; set; } = false;
        public int FrameQueryTimeoutMs { get; set; } = 60000;
        public int HeartbeatDelayMs { get; set; } = 10000;
        public int TunnelEncryptionKeySize { get; set; } = 8;
        public int MaxStaleConnectionAgeMs { get; set; } = 600000;

        #region Reliable Messaging.

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
