namespace NetTunnel.Library.Types
{
    public class NtTunnelInboundConfiguration
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int DataPort { get; set; }
        public List<NtEndpointOutboundConfiguration> OutboundEndpointConfigurations { get; private set; } = new();
        public List<NtEndpointInboundConfiguration> InboundEndpointConfigurations { get; private set; } = new();

        public NtTunnelInboundConfiguration(string name, int dataPort)
        {
            Name = name;
            DataPort = dataPort;
        }

        public NtTunnelInboundConfiguration Clone()
        {
            return new NtTunnelInboundConfiguration(Name, DataPort)
            {
                Id = Id
            };
        }
    }
}
