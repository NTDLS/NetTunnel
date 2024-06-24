using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryGetTunnelStatistics : IRmQuery<QueryGetTunnelStatisticsReply>
    {
    }

    public class QueryGetTunnelStatisticsReply : IRmQueryReply
    {
        public List<TunnelStatisticsDisplay> Statistics { get; set; } = new();

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
