using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryPing : IRmQuery<QueryPingReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public DateTime OriginationTimestamp { get; set; }
        public double? PreviousPing { get; set; }

        public QueryPing()
        {
            TunnelKey = new DirectionalKey();
            OriginationTimestamp = DateTime.UtcNow;
        }

        public QueryPing(DirectionalKey tunnelKey, double? previousPing)
        {
            OriginationTimestamp = DateTime.UtcNow;
            PreviousPing = previousPing;
            TunnelKey = tunnelKey;
        }
    }

    public class QueryPingReply : IRmQueryReply
    {
        public DateTime OriginationTimestamp { get; set; }

        public QueryPingReply(DateTime originationTimestamp)
        {
            OriginationTimestamp = originationTimestamp;
        }

        public QueryPingReply()
        {
        }
    }
}
