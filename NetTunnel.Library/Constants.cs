namespace NetTunnel.Library
{
    public static class Constants
    {
        public const string FriendlyName = "NetTunnel";
        public const string EventSourceName = "NetTunnel";

        public enum NtTrafficType
        {
            Raw,
            Http,
            Https
        }

        public enum NtHttpVerb
        {
            Any,
            Connect,
            Delete,
            Get,
            Head,
            Options,
            Post,
            Put
        }

        public enum NtHttpHeaderAction
        {
            Insert,
            Update,
            Delete,
            Upsert
        }

        public enum NtHttpHeaderType
        {
            None,
            Request,
            Response,
            Any
        }

        public enum NtDirection
        {
            Undefined,
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
            Exception = 0,//An actual exception has been thrown.
            Warning = 1,//Something the user might want to be aware of.
            Verbose = 2, //General status messages.
            Debug = 3, //Super-verbose, debug information.
        }
    }
}
