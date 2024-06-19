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
        {
            int combinedHash = int.MaxValue / 2;

            foreach (var stat in Statistics)
            {
                combinedHash = Utility.CombineHashes(combinedHash, stat.ChangeHash);
            }

            return combinedHash;
        }

        public QueryGetTunnelStatisticsReply()
        {
        }
    }
}
