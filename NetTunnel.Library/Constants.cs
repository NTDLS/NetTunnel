using NetTunnel.Library.Payloads;

namespace NetTunnel.Library
{
    public static class Constants
    {
        public const string FriendlyName = "NetTunnel";

        /// <summary>
        /// The environment variable which gives the service a data folder (and so a configuration) of its own. The tests use it so that
        /// they never read or change the configuration of an installed service.
        /// </summary>
        public const string DataFolderNameVariable = "NETTUNNEL_DATA_NAME";

        /// <summary>
        /// The name of the folder (under the common application data folder) that holds the configuration, users, tunnels and secret key.
        /// </summary>
        public static string DataFolderName
            => Environment.GetEnvironmentVariable(DataFolderNameVariable) is { Length: > 0 } name ? name : FriendlyName;

        /// <summary>
        /// True when the service is using a data folder other than its own (tests), so it must not touch the machine: no event log source.
        /// </summary>
        public static bool IsUsingAlternateDataFolder => DataFolderName != FriendlyName;
        public const string EventSourceName = "NetTunnel";

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
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

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtTrafficType
        {
            Raw,
            Http,
            Https
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtLoginType
        {
            Undefined,
            Service,
            UI
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtUserRole
        {
            Undefined,
            Limited,
            Administrator
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtHttpVerb
        {
            Any,
            Connect,
            Delete,
            Get,
            Head,
            Options,
            Post,
            Put,
            Patch,
            Trace
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtHttpHeaderAction
        {
            Insert,
            Update,
            Delete,
            Upsert
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtHttpHeaderType
        {
            None,
            Request,
            Response,
            Any
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
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

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtTunnelStatus
        {
            Undefined,
            Connecting,
            Established,
            Disconnected,
            Stopped
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public enum NtLogSeverity
        {
            Exception = 0,//An actual exception has been thrown.
            Warning = 1,//Something the user might want to be aware of.
            Verbose = 2, //General status messages.
            Debug = 3, //Super-verbose, debug information.
        }
    }
}
