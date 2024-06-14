namespace NetTunnel.Library.Types
{
    public interface INtTunnelConfiguration
    {
        public Guid TunnelId { get; }
        public string Name { get; }
        public List<NtEndpointConfiguration> EndpointConfigurations { get; }
    }
}
