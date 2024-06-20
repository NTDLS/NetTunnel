using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
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
