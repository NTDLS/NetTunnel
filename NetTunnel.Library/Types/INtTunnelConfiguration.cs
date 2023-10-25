namespace NetTunnel.Library.Types
{
    public interface INtTunnelConfiguration
    {
        public List<NtEndpointOutboundConfiguration> EndpointOutboundConfigurations { get; set; }
        public List<NtEndpointInboundConfiguration> EndpointInboundConfigurations { get; set; }
    }
}
