using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseStatistics : NtActionResponse
    {
        public List<NtTunnelStatistics> Statistics { get; set; } = new();

        /// <summary>
        /// This overall hash is used to determine if there has been an additions/removals of any tunnel or endpoint.
        /// This should be the hash of tunnels, endpoints and http rules.
        /// </summary>
        /// <returns></returns>
        public int AllTunnelIdAndEndpointIdHashes()
            => Statistics.Sum(o => o.ChangeHash);

        public NtActionResponseStatistics()
        {
        }

        public NtActionResponseStatistics(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
