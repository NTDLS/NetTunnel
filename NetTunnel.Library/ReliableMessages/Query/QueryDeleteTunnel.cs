using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryDeleteTunnel : IRmQuery<QueryDeleteTunnelReply>
    {
        public DirectionalKey? TunnelKey { get; set; }

        public QueryDeleteTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }

        public QueryDeleteTunnel()
        {
        }
    }

    public class QueryDeleteTunnelReply : IRmQueryReply
    {
        public QueryDeleteTunnelReply()
        {
        }
    }
}
