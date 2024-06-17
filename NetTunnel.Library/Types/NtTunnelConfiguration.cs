namespace NetTunnel.Library.Types
{
    public class NtTunnelConfiguration
    {
        /// <summary>
        /// The id of the service that owns this tunnel.
        /// </summary>
        public Guid ServiceId { get; set; }
        public Guid TunnelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ManagementPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<NtEndpointConfiguration> Endpoints { get; set; } = new();

        public NtTunnelConfiguration()
        {
        }

        public NtTunnelConfiguration(Guid serviceId, Guid tunnelId, string name, string address, int managementPort, string username, string passwordHash)
        {
            ServiceId = serviceId;
            TunnelId = tunnelId;
            Name = name;
            Address = address;
            ManagementPort = managementPort;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtTunnelConfiguration CloneConfiguration()
        {
            var clone = new NtTunnelConfiguration
            {
                TunnelId = TunnelId,
                Name = Name,
                Address = Address,
                ManagementPort = ManagementPort,
                Username = Username,
                PasswordHash = PasswordHash,
                ServiceId = ServiceId,
            };

            foreach (var endpoint in Endpoints)
            {
                clone.Endpoints.Add(endpoint.CloneConfiguration());
            }

            return clone;
        }
    }
}