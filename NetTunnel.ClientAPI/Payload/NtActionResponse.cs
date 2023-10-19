namespace NetTunnel.ClientAPI.Payload
{
    public class NtActionResponse
    {
        public bool Success { get; set; } = true;
        public string? ExceptionText { get; set; }

        public NtActionResponse() { }

        public NtActionResponse(Exception ex)
        {
            ExceptionText = ex.Message;
            Success = false;
        }
    }
}
