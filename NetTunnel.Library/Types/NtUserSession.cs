namespace NetTunnel.Library.Types
{
    public class NtUserSession
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();
        public DateTime LoginTime { get; private set; } = DateTime.UtcNow;
        public string Username { get; set; }
        public string? ClientIpAddress { get; set; }

        public NtUserSession(string username, string? clientIpAddress)
        {
            Username = username;
            ClientIpAddress = clientIpAddress;
        }
    }
}
