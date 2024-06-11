namespace NetTunnel.Library.Types
{
    public class NtTunnelInboundConfiguration : INtTunnelConfiguration
    {
        public Guid TunnelId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DataPort { get; set; }

        public List<NtEndpointOutboundConfiguration> EndpointOutboundConfigurations { get; set; } = new();

        public List<NtEndpointInboundConfiguration> EndpointInboundConfigurations { get; set; } = new();

        public NtTunnelInboundConfiguration()
        {
        }

        public NtTunnelInboundConfiguration(Guid tunnelId, string name, int dataPort)
        {
            TunnelId = tunnelId;
            Name = name;
            DataPort = dataPort;
        }

        public NtTunnelInboundConfiguration Clone()
        {
            return new NtTunnelInboundConfiguration(TunnelId, Name, DataPort);
        }
    }
}
