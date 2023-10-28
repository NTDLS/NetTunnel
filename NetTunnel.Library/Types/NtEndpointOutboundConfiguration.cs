using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    /// <summary>
    /// The tunnel connector contains information that defines an outbound termination connection from an established endpoint.
    /// </summary>
    public class NtEndpointOutboundConfiguration : INtEndpointConfiguration
    {
        [ProtoMember(1)]
        public Guid PairId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelPairId { get; set; }

        [ProtoMember(3)]
        public string Name { get; set; } = string.Empty;

        [ProtoMember(4)]
        public string Address { get; set; } = string.Empty;

        [ProtoMember(5)]
        public int TransmissionPort { get; set; }

        public NtEndpointOutboundConfiguration() { }

        public NtEndpointOutboundConfiguration(Guid tunnelPairId, Guid pairId, string name, string address, int transmissionPort)
        {
            TunnelPairId = tunnelPairId;
            PairId = pairId;
            Name = name;
            Address = address;
            TransmissionPort = transmissionPort;
        }
    }
}
