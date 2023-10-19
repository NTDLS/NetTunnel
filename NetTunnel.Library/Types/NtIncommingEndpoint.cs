namespace NetTunnel.Library.Types
{
    public class NtIncommingEndpoint
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }

        public NtIncommingEndpoint(string name)
        {
            Name = name;
        }

        public NtIncommingEndpoint Clone()
        {
            return new NtIncommingEndpoint(Name)
            {
                Id = Id
            };
        }
    }
}
