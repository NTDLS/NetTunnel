using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseStatistics : NtActionResponse
    {
        public List<NtTunnelStatistics> Statistics { get; set; } = new();

        public NtActionResponseStatistics() { }

        public NtActionResponseStatistics(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
