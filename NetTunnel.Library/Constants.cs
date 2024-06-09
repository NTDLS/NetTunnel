namespace NetTunnel.Library
{
    public static class Constants
    {
        public const string FriendlyName = "NetTunnel";
        public const string EventSourceName = "NetTunnel";

        public enum NtDirection
        {
            Inbound,
            Outbound
        }

        public enum NtTunnelStatus
        {
            Undefined,
            Connecting,
            Established,
            Disconnected,
            Stopped
        }

        public enum NtLogSeverity
        {
            Debug = 0, //Super-verbose, debug information.
            Verbose = 1, //General status messages.
            Warning = 2, //Something the user might want to be aware of.
            Exception = 3 //An actual exception has been thrown.
        }
    }
}
