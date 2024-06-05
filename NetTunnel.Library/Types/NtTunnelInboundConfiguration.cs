using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    public class NtTunnelInboundConfiguration : INtTunnelConfiguration
    {
        [ProtoMember(1)]
        public Guid TunnelId { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; } = string.Empty;
        [ProtoMember(3)]
        public int DataPort { get; set; }
        [ProtoMember(4)]
        public List<NtEndpointOutboundConfiguration> EndpointOutboundConfigurations { get; set; } = new();
        [ProtoMember(5)]
        public List<NtEndpointInboundConfiguration> EndpointInboundConfigurations { get; set; } = new();

        public NtTunnelInboundConfiguration() { }

        public NtTunnelInboundConfiguration(Guid tunnelId, string name, int dataPort)
        {
            TunnelId = tunnelId;
            Name = name;
            DataPort = dataPort;
        }

        public NtTunnelInboundConfiguration Clone()
        {
            return new NtTunnelInboundConfiguration(TunnelId, Name, DataPort);
        }
    }
}
