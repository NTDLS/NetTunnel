namespace NetTunnel.Library.Types
{
    public class NtTunnelOutboundConfiguration : INtTunnelConfiguration
    {
        public Guid TunnelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ManagementPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<NtEndpointConfiguration> EndpointConfigurations { get; set; } = new();

        public NtTunnelOutboundConfiguration()
        {
        }

        public NtTunnelOutboundConfiguration(Guid tunnelId, string name, string address, int managementPort, string username, string passwordHash)
        {
            TunnelId = tunnelId;
            Name = name;
            Address = address;
            ManagementPort = managementPort;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtTunnelOutboundConfiguration Clone()
        {
            return new NtTunnelOutboundConfiguration(TunnelId, Name, Address, ManagementPort, Username, PasswordHash);
        }
    }
}