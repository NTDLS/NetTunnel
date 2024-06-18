using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryRegisterTunnel : IRmQuery<QueryRegisterTunnelReply>
    {
        public NtTunnelConfiguration Configuration { get; set; } = new();

        public QueryRegisterTunnel()
        {
        }

        public QueryRegisterTunnel(NtTunnelConfiguration configuration)
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
