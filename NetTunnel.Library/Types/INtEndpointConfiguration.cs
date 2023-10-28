namespace NetTunnel.Library.Types
{
    public interface INtEndpointConfiguration
    {
        public Guid PairId { get; }
        public Guid TunnelPairId { get; }
        public string Name { get; }
        public int TransmissionPort { get; }
    }
}
