using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryCreateOutboundTunnel : IRmQuery<QueryCreateOutboundTunnelReply>
    {
        public NtTunnelOutboundConfiguration Configuration { get; set; }

        public QueryCreateOutboundTunnel(NtTunnelOutboundConfiguration configuration)
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
