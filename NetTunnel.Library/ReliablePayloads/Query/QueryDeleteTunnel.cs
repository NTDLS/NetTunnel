using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryDeleteTunnel : IRmQuery<QueryDeleteTunnelReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public QueryDeleteTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }

    public class QueryDeleteTunnelReply : IRmQueryReply
    {
        public QueryDeleteTunnelReply()
        {
        }
    }
}
