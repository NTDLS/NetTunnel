namespace NetTunnel.Library.Payloads
{
    public class ControllerActionResponse
    {
        public Guid SessionId { get; set; }
        public bool Success { get; set; } = true;
        public string? ExceptionText { get; set; }

        public ControllerActionResponse()
        {
        }

        public ControllerActionResponse(Guid sessionId)
        {
            SessionId = sessionId;
        }

        public ControllerActionResponse(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
