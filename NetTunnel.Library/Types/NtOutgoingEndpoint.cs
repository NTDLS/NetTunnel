namespace NetTunnel.Library.Types
{
    public class NtOutgoingEndpoint
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public NtOutgoingEndpoint(string name, string address, int port, string username, string passwordHash)
        {
            Name = name;
            Address = address;
            Port = port;
            Username = username;
            PasswordHash = passwordHash;
        }

        public NtOutgoingEndpoint Clone()
        {
            return new NtOutgoingEndpoint(Name, Address, Port, Username, PasswordHash)
            {
                Id = Id
            };
        }
    }
}