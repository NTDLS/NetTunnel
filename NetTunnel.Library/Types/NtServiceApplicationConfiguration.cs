namespace NetTunnel.Library.Types
{
    public class NtServiceApplicationConfiguration
    {
        public int ManagementPort { get; set; } = 52845;
        public int RSAKeyLength { get; set; } = 2048;
        public int FramebufferSize { get; set; } = 16384;
        public int MaxFrameSize { get; set; } = 134217728;
        public bool DebugLogging { get; set; } = false;
        public int FrameQueryTimeoutMs { get; set; } = 60000;
        public int HeartbeatDelayMs { get; set; } = 10000;
        public int TunnelEncryptionKeySize { get; set; } = 8;
        public int MaxStaleConnectionAgeMs { get; set; } = 600000;
    }
}
