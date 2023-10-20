using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseOutboundTunnels : NtActionResponse
    {
        public List<NtTunnelOutboundConfiguration> Collection { get; set; } = new();

        public NtActionResponseOutboundTunnels() { }

        public NtActionResponseOutboundTunnels(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
