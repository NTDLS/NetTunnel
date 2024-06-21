using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryStopTunnel : IRmQuery<QueryStopTunnelReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public QueryStopTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }

    public class QueryStopTunnelReply : IRmQueryReply
    {
        public QueryStopTunnelReply()
        {
        }
    }
}
