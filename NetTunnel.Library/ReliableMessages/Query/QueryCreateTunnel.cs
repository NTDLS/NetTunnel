using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryCreateTunnel : IRmQuery<QueryCreateTunnelReply>
    {
        public TunnelConfiguration Configuration { get; set; }

        public QueryCreateTunnel(TunnelConfiguration configuration)
        {
            Configuration = configuration;
        }

    }

    public class QueryCreateTunnelReply : IRmQueryReply
    {
        public QueryCreateTunnelReply()
        {
        }
    }
}
