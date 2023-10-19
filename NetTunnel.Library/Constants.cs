namespace NetTunnel.Library
{
    public static class Constants
    {
        public const string FriendlyName = "NetTunnel";

        public enum EndpointDirection
        {
            /// <summary>
            /// This is a listening endpoint that is expected to be accessible from the internet.
            /// </summary>
            Incomming,
            /// <summary>
            /// This is a outgoing/connecting endpoint that may be totally inaccessible from the outside internet.
            /// </summary>
            Outgoing
        }
    }
}
