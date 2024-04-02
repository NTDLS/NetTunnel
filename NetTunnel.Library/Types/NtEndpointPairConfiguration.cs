using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    public class NtEndpointPairConfiguration
    {
        [ProtoMember(1)]
        public NtEndpointInboundConfiguration Inbound { get; private set; } = new();
        [ProtoMember(2)]
        public NtEndpointOutboundConfiguration Outbound { get; private set; } = new();

        public NtEndpointPairConfiguration() { }

        public NtEndpointPairConfiguration(NtEndpointInboundConfiguration inbound, NtEndpointOutboundConfiguration outbound)
        {
            Inbound = inbound;
            Outbound = outbound;
        }
    }
}
