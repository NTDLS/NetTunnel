using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseStatistics : NtActionResponse
    {
        public List<NtTunnelStatistics> Statistics { get; set; } = new();

        /// <summary>
        /// This overall hash is used to determine if there has been an additions/removals of any tunnel or endpoint.
        /// </summary>
        /// <returns></returns>
        public int AllTunnelIdAndEndpointIdHashes()
        {
            int rollingTotal = 0;

            foreach (var item in Statistics)
            {
                rollingTotal += item.ChangeHash;

                item.EndpointStatistics
                    .ForEach(kvp => rollingTotal += kvp.ChangeHash);
            }
            return rollingTotal;
        }

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
