using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class NtEndpointStatistics
    {
        public int CurrentConnections { get; set; }
        public int TotalConnections { get; set; }
        public NtDirection Direction { get; set; }
        public Guid TunnelPairId { get; set; }
        public Guid EndpointPairId { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
    }
}
