using NetTunnel.Service;

namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponseUsers : NtActionResponse
    {
        public List<NtUser> Collection { get; set; } = new();

        public NtActionResponseUsers() { }

        public NtActionResponseUsers(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
