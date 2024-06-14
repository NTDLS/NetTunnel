using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryCreateTunnel : IRmQuery<QueryCreateOutboundTunnelReply>
    {
        public NtTunnelConfiguration Configuration { get; set; }

        public QueryCreateTunnel(NtTunnelConfiguration configuration)
        {
            Configuration = configuration;
        }

    }

    public class QueryCreateOutboundTunnelReply : IRmQueryReply
    {
        public QueryCreateOutboundTunnelReply()
        {
        }
    }
}
