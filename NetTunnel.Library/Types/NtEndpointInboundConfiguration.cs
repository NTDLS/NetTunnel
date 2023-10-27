using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    /// <summary>
    /// The inbound endpoint contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfiguration: INtEndpointConfiguration
    {
        [ProtoMember(1)]
        public Guid PairId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelPairId { get; set; }

        [ProtoMember(3)]
        public string Name { get; set; } = string.Empty;

        [ProtoMember(4)]
        public int Port { get; set; }

        public NtEndpointInboundConfiguration() { }

        public NtEndpointInboundConfiguration(Guid tunnelPairId, Guid pairId, string name, int port)
        {
            TunnelPairId = tunnelPairId;
            PairId = pairId;
            Name = name;
            Port = port;
        }
    }
}
