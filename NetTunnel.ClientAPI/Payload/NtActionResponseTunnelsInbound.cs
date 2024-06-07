using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseTunnelsInbound : NtActionResponse
    {
        public List<NtTunnelInboundConfiguration> Collection { get; set; } = new();

        public NtActionResponseTunnelsInbound()
        {
        }

        public NtActionResponseTunnelsInbound(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
