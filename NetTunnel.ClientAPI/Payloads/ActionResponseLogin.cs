namespace NetTunnel.Library.Payloads
{
    public class ActionResponseLogin : ActionResponse
    {
        public Guid SessionId { get; set; } = Guid.Empty;

        public ActionResponseLogin(Guid sessionId)
        {
            SessionId = sessionId;
        }

        public ActionResponseLogin(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
