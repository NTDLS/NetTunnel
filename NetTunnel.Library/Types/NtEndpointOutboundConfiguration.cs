namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The tunnel connector contains information that defines an outbound termination connection from an established endpoint.
    /// </summary>
    public class NtEndpointOutboundConfiguration
    {
        public Guid PairId { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }

        public NtEndpointOutboundConfiguration(Guid pairId, string name, string address, int port)
        {
            PairId = pairId;
            Name = name;
            Address = address;
            Port = port;
        }
    }
}
