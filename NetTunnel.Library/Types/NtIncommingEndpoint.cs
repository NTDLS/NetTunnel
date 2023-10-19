namespace NetTunnel.Library.Types
{
    public class NtIncommingEndpoint
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public NtIncommingEndpoint Clone()
        {
            return new NtIncommingEndpoint
            {
                Name = Name,
                Username = Username,
                PasswordHash = PasswordHash
            };
        }
    }
}
