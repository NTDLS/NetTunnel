using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryGetOutboundTunnels : IRmQuery<GetOutboundTunnelsReply>
    {
    }

    public class GetOutboundTunnelsReply : IRmQueryReply
    {
        public List<NtTunnelOutboundConfiguration> Collection { get; set; } = new();

        public GetOutboundTunnelsReply()
        {
        }
    }
}
