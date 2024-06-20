using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryRegisterTunnel : IRmQuery<QueryRegisterTunnelReply>
    {
        public TunnelConfiguration Configuration { get; set; } = new();

        public QueryRegisterTunnel()
        {
        }

        public QueryRegisterTunnel(TunnelConfiguration configuration)
        {
            Configuration = configuration;
        }
    }

    public class QueryRegisterTunnelReply : IRmQueryReply
    {

        public QueryRegisterTunnelReply()
        {
        }
    }
}
