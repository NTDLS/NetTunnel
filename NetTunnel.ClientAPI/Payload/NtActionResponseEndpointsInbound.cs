using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseEndpointsInbound : NtActionResponse
    {
        public List<NtEndpointInboundConfiguration> Collection { get; set; } = new();

        public NtActionResponseEndpointsInbound()
        {
        }

        public NtActionResponseEndpointsInbound(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
