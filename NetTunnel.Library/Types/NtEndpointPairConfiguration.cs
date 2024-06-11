namespace NetTunnel.Library.Types
{
    public class NtEndpointPairConfiguration
    {
        public NtEndpointInboundConfiguration Inbound { get; private set; } = new();

        public NtEndpointOutboundConfiguration Outbound { get; private set; } = new();

        public NtEndpointPairConfiguration() { }

        public NtEndpointPairConfiguration(NtEndpointInboundConfiguration inbound, NtEndpointOutboundConfiguration outbound)
        {
            Inbound = inbound;
            Outbound = outbound;
        }
    }
}
