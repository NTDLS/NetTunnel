using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    public class TunnelPropertiesDisplay
    {
        public bool IsLoggedIn { get; set; }
        public bool KeepRunning { get; set; }
        public string ClientIpAddress { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
        public DateTime LoginTime { get; set; }
        public bool SecureKeyExchangeIsComplete { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string LoggedInUserName { get; set; } = string.Empty;
        public Guid ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Endpoints { get; set; }
        public string OutboundAddress { get; set; } = string.Empty;
        public int OutboundPort { get; set; }
        public string OutboundUsername { get; set; } = string.Empty;
        public string InboundAddress { get; set; } = string.Empty;
        public int InboundPort { get; set; }
        public string InboundUsername { get; set; } = string.Empty;
        public Guid ConnectionId { get; set; }
        public int KeyLength { get; set; }
        public string KeyHash { get; set; } = string.Empty;
        public double? PingTime { get; set; }
        public NtTunnelStatus Status { get; set; }
        public ulong CurrentConnections { get; set; }
        public ulong TotalConnections { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public DirectionalKey? TunnelKey { get; set; }
    }
}
