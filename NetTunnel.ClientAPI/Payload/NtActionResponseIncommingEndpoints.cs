using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseIncommingEndpoints : NtActionResponse
    {
        public List<NtIncommingEndpointConfig> Collection { get; set; } = new();

        public NtActionResponseIncommingEndpoints() { }

        public NtActionResponseIncommingEndpoints(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
