namespace NetTunnel.Library.Payloads
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
        public int ServicePort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public TunnelConfiguration()
        {
        }

        public TunnelConfiguration(Guid serviceId, Guid tunnelId, string name, string address, int servicePort, string username, string passwordHash)
        {
            ServiceId = serviceId;
            TunnelId = tunnelId;
            Name = name;
            Address = address;
            ServicePort = servicePort;
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
                ServicePort = ServicePort,
                Username = Username,
                PasswordHash = PasswordHash,
                ServiceId = ServiceId,
            };

            return clone;
        }
    }
}