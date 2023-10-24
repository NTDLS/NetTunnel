using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    /// <summary>
    /// The tunnel connector contains information that defines an outbound termination connection from an established endpoint.
    /// </summary>
    public class NtEndpointOutboundConfiguration
    {
        [ProtoMember(1)]
        public Guid PairId { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; } = string.Empty;
        [ProtoMember(3)]
        public string Address { get; set; } = string.Empty;
        [ProtoMember(4)]
        public int Port { get; set; }

        public NtEndpointOutboundConfiguration() { }

        public NtEndpointOutboundConfiguration(Guid pairId, string name, string address, int port)
        {
            PairId = pairId;
            Name = name;
            Address = address;
            Port = port;
        }
    }
}
