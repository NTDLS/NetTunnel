namespace NetTunnel.Library.Types
{
    public class NtTunnelInboundConfiguration : INtTunnelConfiguration
    {
        public Guid TunnelId { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<NtEndpointOutboundConfiguration> EndpointOutboundConfigurations { get; set; } = new();

        public List<NtEndpointInboundConfiguration> EndpointInboundConfigurations { get; set; } = new();

        public NtTunnelInboundConfiguration()
        {
        }

        public NtTunnelInboundConfiguration(Guid tunnelId, string name)
        {
            TunnelId = tunnelId;
            Name = name;
        }

        public NtTunnelInboundConfiguration Clone()
        {
            return new NtTunnelInboundConfiguration(TunnelId, Name);
        }
    }
}
