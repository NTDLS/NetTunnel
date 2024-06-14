using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryCreateOutboundTunnel : IRmQuery<QueryCreateOutboundTunnelReply>
    {
        public NtTunnelConfiguration Configuration { get; set; }

        public QueryCreateOutboundTunnel(NtTunnelConfiguration configuration)
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
