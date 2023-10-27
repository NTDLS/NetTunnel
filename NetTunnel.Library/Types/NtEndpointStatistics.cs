using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class NtEndpointStatistics
    {
        public ulong CurrentConnections { get; set; }
        public ulong TotalConnections { get; set; }
        public NtDirection Direction { get; set; }
        public Guid TunnelPairId { get; set; }
        public Guid EndpointPairId { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public double BytesReceivedKb => BytesReceived / 1024.0;
        public double BytesSentKb => BytesSent / 1024.0;
    }
}
