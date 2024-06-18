namespace NetTunnel.Library.Types
{
    public class TunnelConfiguration
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
        public List<EndpointConfiguration> Endpoints { get; set; } = new();

        public TunnelConfiguration()
        {
        }

        public TunnelConfiguration(Guid serviceId, Guid tunnelId, string name, string address, int managementPort, string username, string passwordHash)
        {
            ServiceId = serviceId;
            TunnelId = tunnelId;
            Name = name;
            Address = address;
            ManagementPort = managementPort;
            Username = username;
            PasswordHash = passwordHash;
        }

        public TunnelConfiguration CloneConfiguration()
        {
            var clone = new TunnelConfiguration
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