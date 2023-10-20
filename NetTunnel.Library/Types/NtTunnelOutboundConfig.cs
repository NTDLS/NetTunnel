namespace NetTunnel.Library.Types
{
    public class NtTunnelOutboundConfig
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public List<NtEndpointOutboundConfig> Connectors { get; set; } = new();
        public List<NtEndpointInboundConfig> Listeners { get; set; } = new();

        public NtTunnelOutboundConfig(string name, string address, int port, string username, string passwordHash)
        {
            Name = name;
            Address = address;
            Port = port;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtTunnelOutboundConfig Clone()
        {
            return new NtTunnelOutboundConfig(Name, Address, Port, Username, PasswordHash)
            {
                Id = Id
            };
        }
    }
}