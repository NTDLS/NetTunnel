using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryPing : IRmQuery<QueryPingReply>
    {
        public DateTime OriginationTimestamp { get; set; }

        public QueryPing()
        {
            OriginationTimestamp = DateTime.UtcNow;
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
