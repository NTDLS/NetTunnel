using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseOutgoingEndpoints : NtActionResponse
    {
        public List<NtOutgoingEndpointConfig> Collection { get; set; } = new();

        public NtActionResponseOutgoingEndpoints() { }

        public NtActionResponseOutgoingEndpoints(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
