using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryGetTunnelStatistics : IRmQuery<QueryGetTunnelStatisticsReply>
    {
    }

    public class QueryGetTunnelStatisticsReply : IRmQueryReply
    {
        public List<TunnelStatistics> Statistics { get; set; } = new();

        public int AllTunnelIdAndEndpointIdHashes()
            => Statistics.Sum(o => o.ChangeHash);

        public QueryGetTunnelStatisticsReply()
        {
        }
    }
}
