namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The inbound endpoint contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfiguration
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int ListenPort { get; set; }
        public Guid TunnelId { get; set; }

        public NtEndpointInboundConfiguration(string name, int dataPort)
        {
            Name = name;
            ListenPort = dataPort;
        }

        public NtEndpointInboundConfiguration Clone()
        {
            return new NtEndpointInboundConfiguration(Name, ListenPort)
            {
                Id = Id
            };
        }

    }
}
