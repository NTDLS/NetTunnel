namespace NetTunnel.Library.Types
{
    public class NtIncommingEndpoint
    {
        public NtEndpoint Endpoint { get; set; } = new();

        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

    }
}
