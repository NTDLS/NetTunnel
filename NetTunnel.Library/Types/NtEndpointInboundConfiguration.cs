namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The inbound endpoint contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfiguration
    {
        public Guid PairId { get; private set; }
        public string Name { get; private set; }
        public int Port { get; private set; }

        public NtEndpointInboundConfiguration(Guid pairId, string name, int port)
        {
            PairId = pairId;
            Name = name;
            Port = port;
        }
    }
}
