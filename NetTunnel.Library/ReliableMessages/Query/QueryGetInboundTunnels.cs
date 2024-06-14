using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryGetInboundTunnels : IRmQuery<GetInboundTunnelsReply>
    {
    }

    public class GetInboundTunnelsReply : IRmQueryReply
    {
        public List<NtTunnelInboundConfiguration> Collection { get; set; } = new();

        public GetInboundTunnelsReply()
        {
        }
    }
}
