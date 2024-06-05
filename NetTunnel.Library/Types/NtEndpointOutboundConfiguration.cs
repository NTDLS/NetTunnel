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
        public Guid EndpointId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelId { get; set; }

        [ProtoMember(3)]
        public string Name { get; set; } = string.Empty;

        [ProtoMember(4)]
        public string Address { get; set; } = string.Empty;

        [ProtoMember(5)]
        public int TransmissionPort { get; set; }

        public NtEndpointOutboundConfiguration() { }

        public NtEndpointOutboundConfiguration(Guid tunnelId, Guid endpointId, string name, string address, int transmissionPort)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Name = name;
            Address = address;
            TransmissionPort = transmissionPort;
        }
    }
}
