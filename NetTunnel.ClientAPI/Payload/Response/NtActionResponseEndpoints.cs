using NetTunnel.Library.Types;

namespace NetTunnel.ClientAPI.Payload.Response
{
    public class NtActionResponseEndpoints : NtActionResponse
    {
        public Guid SessionId { get; set; } = Guid.Empty;

        public List<NtEndpoint> Collection { get; set; } = new();

        public NtActionResponseEndpoints()
        {
        }

        public NtActionResponseEndpoints(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
