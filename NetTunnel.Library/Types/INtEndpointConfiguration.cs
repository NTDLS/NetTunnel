namespace NetTunnel.Library.Types
{
    public interface INtEndpointConfiguration
    {
        public Guid EndpointId { get; }
        public Guid TunnelId { get; }
        public string Name { get; }
        public int TransmissionPort { get; }
    }
}
