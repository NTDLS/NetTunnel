namespace NetTunnel.Library.Types
{
    public class NtUserSession
    {
        public Guid ConnectionId { get; set; }
        public DateTime LoginTime { get; private set; } = DateTime.UtcNow;
        public string Username { get; set; }
        public string? ClientIpAddress { get; set; }

        public NtUserSession(Guid connectionId, string username, string? clientIpAddress)
        {
            ConnectionId = connectionId;
            Username = username.ToLower();
            ClientIpAddress = clientIpAddress;
        }
    }
}
