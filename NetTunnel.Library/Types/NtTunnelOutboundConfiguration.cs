namespace NetTunnel.Library.Types
{
    public class NtTunnelOutboundConfiguration : INtTunnelConfiguration
    {
        public Guid PairId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<NtEndpointOutboundConfiguration> EndpointOutboundConfigurations { get; set; } = new();
        public List<NtEndpointInboundConfiguration> EndpointInboundConfigurations { get; set; } = new();

        public NtTunnelOutboundConfiguration() { }

        public NtTunnelOutboundConfiguration(Guid pairId, string name, string address, int managementPort, int dataPort, string username, string passwordHash)
        {
            PairId = pairId;
            Name = name;
            Address = address;
            ManagementPort = managementPort;
            DataPort = dataPort;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtTunnelOutboundConfiguration Clone()
        {
            return new NtTunnelOutboundConfiguration(PairId, Name, Address, ManagementPort, DataPort, Username, PasswordHash);
        }
    }
}