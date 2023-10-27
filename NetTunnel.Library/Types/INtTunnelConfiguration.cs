namespace NetTunnel.Library.Types
{
    public interface INtTunnelConfiguration
    {
        public Guid PairId { get; }
        public string Name { get; }
        public List<NtEndpointOutboundConfiguration> EndpointOutboundConfigurations { get; }
        public List<NtEndpointInboundConfiguration> EndpointInboundConfigurations { get; }
    }
}
