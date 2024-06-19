using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class TunnelDisplay
    {
        public Guid ServiceId { get; set; }
        public Guid TunnelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ManagementPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<EndpointDisplay> Endpoints { get; set; } = new();
        public NtDirection Direction { get; set; }

        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey TunnelKey => new DirectionalKey(TunnelId, Direction);

        public TunnelDisplay()
        {
        }
    }
}