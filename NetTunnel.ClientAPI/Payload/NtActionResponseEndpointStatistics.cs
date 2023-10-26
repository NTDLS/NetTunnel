using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseEndpointStatistics : NtActionResponse
    {
        public List<NtEndpointStatistics> Statistics { get; set; } = new();

        public NtActionResponseEndpointStatistics() { }

        public NtActionResponseEndpointStatistics(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
