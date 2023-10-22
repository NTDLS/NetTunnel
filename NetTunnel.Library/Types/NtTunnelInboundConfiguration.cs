using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    public class NtTunnelInboundConfiguration
    {
        [ProtoMember(1)]
        public Guid PairId { get; private set; }
        [ProtoMember(2)]
        public string Name { get; set; } = string.Empty;
        [ProtoMember(3)]
        public int DataPort { get; set; }
        [ProtoMember(4)]
        public List<NtEndpointOutboundConfiguration> OutboundEndpointConfigurations { get; private set; } = new();
        [ProtoMember(5)]
        public List<NtEndpointInboundConfiguration> InboundEndpointConfigurations { get; private set; } = new();

        public NtTunnelInboundConfiguration() { }

        public NtTunnelInboundConfiguration(Guid pairId, string name, int dataPort)
        {
            PairId = pairId;
            Name = name;
            DataPort = dataPort;
        }

        public NtTunnelInboundConfiguration Clone()
        {
            return new NtTunnelInboundConfiguration(PairId, Name, DataPort);
        }
    }
}
