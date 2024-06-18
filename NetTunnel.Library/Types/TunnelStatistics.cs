using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class TunnelStatistics
    {
        /// <summary>
        /// Used to determine if anything has changed.
        /// </summary>
        public int ChangeHash { get; set; }
        public List<EndpointStatistics> EndpointStatistics { get; set; } = new();
        public NtTunnelStatus Status { get; set; }
        public ulong CurrentConnections { get; set; }
        public ulong TotalConnections { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public Guid TunnelId { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public double BytesReceivedKb => BytesReceived / 1024.0;
        public double BytesSentKb => BytesSent / 1024.0;
    }
}
