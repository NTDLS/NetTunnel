using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryGetTunnels : IRmQuery<QueryGetTunnelsReply>
    {
    }

    public class QueryGetTunnelsReply : IRmQueryReply
    {
        public List<TunnelDisplay> Collection { get; set; } = new();

        public QueryGetTunnelsReply()
        {
        }
    }
}
