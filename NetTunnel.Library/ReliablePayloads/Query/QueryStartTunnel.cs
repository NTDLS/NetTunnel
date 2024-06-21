using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryStartTunnel : IRmQuery<QueryStartTunnelReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public QueryStartTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }

    public class QueryStartTunnelReply : IRmQueryReply
    {
        public QueryStartTunnelReply()
        {
        }
    }
}
