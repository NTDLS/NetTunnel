namespace NetTunnel.Library.Types
{
    public class NtIncomingEndpointConfig
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int DataPort { get; set; }

        public NtIncomingEndpointConfig(string name, int dataPort)
        {
            Name = name;
            DataPort = dataPort;    
        }

        public NtIncomingEndpointConfig Clone()
        {
            return new NtIncomingEndpointConfig(Name, DataPort)
            {
                Id = Id
            };
        }
    }
}
