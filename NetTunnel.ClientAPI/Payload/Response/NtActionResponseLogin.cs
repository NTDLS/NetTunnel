namespace NetTunnel.ClientAPI.Payload.Response
{
    public class NtActionResponseLogin : NtActionResponse
    {
        public Guid SessionId { get; set; } = Guid.Empty;

        public NtActionResponseLogin()
        {
        }

        public NtActionResponseLogin(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
