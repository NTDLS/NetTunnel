using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseTunnels : NtActionResponse
    {
        public List<NtTunnelConfiguration> Collection { get; set; } = new();

        public NtActionResponseTunnels()
        {
        }

        public NtActionResponseTunnels(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
