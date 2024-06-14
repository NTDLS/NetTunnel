using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryGetTunnels : IRmQuery<GetOutboundTunnelsReply>
    {
    }

    public class GetOutboundTunnelsReply : IRmQueryReply
    {
        public List<NtTunnelConfiguration> Collection { get; set; } = new();

        public GetOutboundTunnelsReply()
        {
        }
    }
}
