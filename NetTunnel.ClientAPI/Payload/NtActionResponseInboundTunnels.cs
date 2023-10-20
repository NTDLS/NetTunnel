using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseInboundTunnels : NtActionResponse
    {
        public List<NtTunnelInboundConfiguration> Collection { get; set; } = new();

        public NtActionResponseInboundTunnels() { }

        public NtActionResponseInboundTunnels(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
