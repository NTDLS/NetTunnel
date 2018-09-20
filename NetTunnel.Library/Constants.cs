namespace NetTunnel.Library
{
    public static class Constants
    {
        public const string RegsitryKey = "SOFTWARE\\NetworkDLS\\NetTunnel";
        public const string ServiceName = "NetworkDLSNetTunnelService";
        public const string TitleCaption = "NetTunnel";
        public const int DefaultManagementPort = 12455;
        public const int DefaultInitialBufferSize = 4096;
        public const int DefaultMaxBufferSize = 1048576;
        public const int DefaultAcceptBacklogSize = 10;
        public const string TunnelConfigFileName = "tunnel.json";
        public const string ServerConfigFileName = "service.json";

        public const int PayloadDelimiter = 122455788;
        public const int DefaultBufferSize = 1024;
        public const int PayloadHeaderSize = 10;
        public const int DefaultMinMsgSize = 0;
        public const int DefaultMaxMsgSize = 1024 * 1024;
    }

    public enum BindingProtocal
    {
        Pv4,
        Pv6
    }
}
