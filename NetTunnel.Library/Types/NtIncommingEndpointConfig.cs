namespace NetTunnel.Library.Types
{
    public class NtIncommingEndpointConfig
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }

        public NtIncommingEndpointConfig(string name)
        {
            Name = name;
        }

        public NtIncommingEndpointConfig Clone()
        {
            return new NtIncommingEndpointConfig(Name)
            {
                Id = Id
            };
        }
    }
}
