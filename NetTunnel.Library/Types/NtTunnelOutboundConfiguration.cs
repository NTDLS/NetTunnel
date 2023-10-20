namespace NetTunnel.Library.Types
{
    public class NtTunnelOutboundConfiguration
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public List<NtEndpointOutboundConfiguration> OutboundEndpointConfigurations { get; private set; } = new();
        public List<NtEndpointInboundConfiguration> InboundEndpointConfigurations { get; private set; } = new();

        public NtTunnelOutboundConfiguration(string name, string address, int managementPort, int dataPort, string username, string passwordHash)
        {
            Name = name;
            Address = address;
            ManagementPort = managementPort;
            DataPort = dataPort;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtTunnelOutboundConfiguration Clone()
        {
            return new NtTunnelOutboundConfiguration(Name, Address, ManagementPort, DataPort, Username, PasswordHash)
            {
                Id = Id
            };
        }
    }
}