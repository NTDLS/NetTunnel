using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryCreateInboundTunnel : IRmQuery<QueryCreateInboundTunnelReply>
    {
        public NtTunnelInboundConfiguration Configuration { get; set; }

        public QueryCreateInboundTunnel(NtTunnelInboundConfiguration configuration)
        {
            Configuration = configuration;
        }

    }

    public class QueryCreateInboundTunnelReply : IRmQueryReply
    {
        public QueryCreateInboundTunnelReply()
        {
        }
    }
}
