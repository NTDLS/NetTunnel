using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseTunnelsOutbound : NtActionResponse
    {
        public List<NtTunnelOutboundConfiguration> Collection { get; set; } = new();

        public NtActionResponseTunnelsOutbound()
        {
        }

        public NtActionResponseTunnelsOutbound(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
