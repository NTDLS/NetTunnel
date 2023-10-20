namespace NetTunnel.Library.Types
{
    public class NtTunnelInboundConfig
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int DataPort { get; set; }
        public List<NtEndpointOutboundConfig> Connectors { get; set; } = new();
        public List<NtEndpointInboundConfig> Listeners { get; set; } = new();

        public NtTunnelInboundConfig(string name, int dataPort)
        {
            Name = name;
            DataPort = dataPort;
        }

        public NtTunnelInboundConfig Clone()
        {
            return new NtTunnelInboundConfig(Name, DataPort)
            {
                Id = Id
            };
        }
    }
}
