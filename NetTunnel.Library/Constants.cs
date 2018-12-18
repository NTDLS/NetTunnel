namespace NetTunnel.Library
{
    public static class Constants
    {
        public const string CommonRegsitryKey = "SOFTWARE\\NetworkDLS\\NetTunnel";
        public const string ServerRegsitryKey = CommonRegsitryKey + "\\Server";
        public const string ClientRegsitryKey = CommonRegsitryKey + "\\Client";
        public const string ClientServiceName = "NetTunnelClientService";
        public const string ServerServiceName = "NetTunnelServerService";
        public const string TitleCaption = "NetTunnel";
        public const int DefaultManagementPort = 12455;
        public const int DefaultInitialBufferSize = 4096;
        public const int DefaultMaxBufferSize = 1048576;
        public const int DefaultAcceptBacklogSize = 5;
        public const string TunnelConfigFileName = "tunnel.json";
        public const string ServiceConfigFileName = "service.json";

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
