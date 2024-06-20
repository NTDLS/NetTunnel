using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
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
