using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseIncomingEndpoints : NtActionResponse
    {
        public List<NtIncomingEndpointConfig> Collection { get; set; } = new();

        public NtActionResponseIncomingEndpoints() { }

        public NtActionResponseIncomingEndpoints(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
