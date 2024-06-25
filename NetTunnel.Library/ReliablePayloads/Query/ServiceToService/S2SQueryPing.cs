using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.ServiceToService
{
    public class S2SQueryPing : IRmQuery<S2SQueryPingReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public DateTime OriginationTimestamp { get; set; }
        public double? PreviousPing { get; set; }

        public S2SQueryPing()
        {
            TunnelKey = new DirectionalKey();
            OriginationTimestamp = DateTime.UtcNow;
        }

        public S2SQueryPing(DirectionalKey tunnelKey, double? previousPing)
        {
            OriginationTimestamp = DateTime.UtcNow;
            PreviousPing = previousPing;
            TunnelKey = tunnelKey;
        }
    }

    public class S2SQueryPingReply : IRmQueryReply
    {
        public DateTime OriginationTimestamp { get; set; }

        public S2SQueryPingReply(DateTime originationTimestamp)
        {
            OriginationTimestamp = originationTimestamp;
        }

        public S2SQueryPingReply()
        {
        }
    }
}
