namespace NetTunnel.Library.Types
{
    public class NtTunnelInboundConfiguration
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public int DataPort { get; set; }
        public List<NtEndpointOutboundConfiguration> OutboundEndpointConfigurations { get; private set; } = new();
        public List<NtEndpointInboundConfiguration> InboundEndpointConfigurations { get; private set; } = new();

        public NtTunnelInboundConfiguration(Guid id, string name, int dataPort)
        {
            Id = id;
            Name = name;
            DataPort = dataPort;
        }

        public NtTunnelInboundConfiguration Clone()
        {
            return new NtTunnelInboundConfiguration(Id, Name, DataPort);
        }
    }
}
