using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    /// <summary>
    /// The inbound endpoint contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfiguration : INtEndpointConfiguration
    {
        [ProtoMember(1)]
        public Guid EndpointId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelId { get; set; }

        [ProtoMember(3)]
        public string Name { get; set; } = string.Empty;

        [ProtoMember(4)]
        public int TransmissionPort { get; set; }

        public NtEndpointInboundConfiguration() { }

        public NtEndpointInboundConfiguration(Guid tunnelId, Guid endpointId, string name, int transmissionPort)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Name = name;
            TransmissionPort = transmissionPort;
        }
    }
}
