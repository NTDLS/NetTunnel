using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    public class TunnelStatisticsDisplay
    {
        /// <summary>
        /// Used to determine if anything has changed.
        /// </summary>
        public int ChangeHash { get; set; }
        public double? PingTime { get; set; }
        public List<EndpointStatisticsDisplay> EndpointStatistics { get; set; } = new();
        public NtTunnelStatus Status { get; set; }
        public ulong CurrentConnections { get; set; }
        public ulong TotalConnections { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public Guid TunnelId { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey? TunnelKey { get; set; }
    }
}
