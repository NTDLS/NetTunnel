using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseStatistics : NtActionResponse
    {
        public List<NtTunnelStatistics> Statistics { get; set; } = new();

        /// <summary>
        /// This overall hash is used to determine if there has been an addition/removal of any tunnel or endpoint.
        /// </summary>
        /// <returns></returns>
        public int AllPairIdHashs()
        {
            int rollingTotal = 0;

            foreach (var item in Statistics)
            {
                rollingTotal += item.TunnelPairId.GetHashCode();
                foreach (var endpoint in item.EndpointStatistics)
                {
                    rollingTotal += endpoint.TunnelPairId.GetHashCode();
                    rollingTotal += endpoint.EndpointPairId.GetHashCode();
                }
            }
            return rollingTotal;
        }

        public NtActionResponseStatistics() { }

        public NtActionResponseStatistics(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
