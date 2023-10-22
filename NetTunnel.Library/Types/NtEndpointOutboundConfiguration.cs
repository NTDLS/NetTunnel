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
        public Guid PairId { get; private set; }
        [ProtoMember(2)]
        public string Name { get; private set; } = string.Empty;
        [ProtoMember(3)]
        public string Address { get; private set; } = string.Empty;
        [ProtoMember(4)]
        public int Port { get; private set; }

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
