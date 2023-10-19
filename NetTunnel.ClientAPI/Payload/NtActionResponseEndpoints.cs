using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseEndpoints : NtActionResponse
    {
        public List<NtEndpoint> Collection { get; set; } = new();

        public NtActionResponseEndpoints() { }

        public NtActionResponseEndpoints(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
