namespace NetTunnel.Library.Payloads
{
    public class ActionResponse
    {
        public bool Success { get; set; } = true;
        public string? ExceptionText { get; set; }

        public ActionResponse()
        {
        }

        public ActionResponse(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
