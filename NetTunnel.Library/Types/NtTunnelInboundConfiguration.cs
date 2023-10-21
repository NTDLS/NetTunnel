namespace NetTunnel.Library.Types
{
    public class NtTunnelInboundConfiguration
    {
        public Guid PairId { get; private set; }
        public string Name { get; set; }
        public int DataPort { get; set; }
        public List<NtEndpointOutboundConfiguration> OutboundEndpointConfigurations { get; private set; } = new();
        public List<NtEndpointInboundConfiguration> InboundEndpointConfigurations { get; private set; } = new();

        public NtTunnelInboundConfiguration(Guid pairId, string name, int dataPort)
        {
            PairId = pairId;
            Name = name;
            DataPort = dataPort;
        }

        public NtTunnelInboundConfiguration Clone()
        {
            return new NtTunnelInboundConfiguration(PairId, Name, DataPort);
        }
    }
}
