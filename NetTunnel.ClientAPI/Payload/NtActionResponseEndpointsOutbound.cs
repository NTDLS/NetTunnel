using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseEndpointsOutbound : NtActionResponse
    {
        public List<NtEndpointConfiguration> Collection { get; set; } = new();

        public NtActionResponseEndpointsOutbound()
        {
        }

        public NtActionResponseEndpointsOutbound(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
