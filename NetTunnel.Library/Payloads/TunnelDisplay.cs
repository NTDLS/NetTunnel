using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    public class TunnelDisplay
    {
        public Guid ServiceId { get; set; }
        public Guid TunnelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int ServicePort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<EndpointConfiguration> Endpoints { get; set; } = new();

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey TunnelKey => new(TunnelId, Direction);

        public TunnelDisplay()
        {
        }
    }
}