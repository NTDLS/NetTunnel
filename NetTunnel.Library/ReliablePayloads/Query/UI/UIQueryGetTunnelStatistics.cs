using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryGetTunnelStatistics : IRmQuery<UIQueryGetTunnelStatisticsReply>
    {
    }

    public class UIQueryGetTunnelStatisticsReply : IRmQueryReply
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

        public UIQueryGetTunnelStatisticsReply()
        {
        }
    }
}
