using NetTunnel.Library.Payloads;

namespace NetTunnel.Library
{
    public static class Constants
    {
        public const string FriendlyName = "NetTunnel";
        public const string EventSourceName = "NetTunnel";

        public enum NtHTTPHeaderResult
        {
            WaitOnData,
            Present,
            NotPresent
        }

        public enum NtEdgeStatus
        {
            Normal, //This is a persistent connection.
            New, //This is a new connection.
            Expire //The connection is gone.
        }

        public class NtEdgeState
        {
            public NtEdgeStatus Status { get; set; }
            public DirectionalKey TunnelKey { get; set; } = new();
            public DirectionalKey EndpointKey { get; set; } = new();
            public Guid EdgeId { get; set; }
        }

        public enum NtTrafficType
        {
            Raw,
            Http,
            Https
        }

        public enum NtLoginType
        {
            Undefined,
            Service,
            UI
        }

        public enum NtUserRole
        {
            Undefined,
            Limited,
            Administrator
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

        public static NtDirection SwapDirection(NtDirection direction)
        {
            if (direction == NtDirection.Inbound)
            {
                return NtDirection.Outbound;
            }
            if (direction == NtDirection.Outbound)
            {
                return NtDirection.Inbound;
            }
            throw new Exception("The direction cannot be reversed.");
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
