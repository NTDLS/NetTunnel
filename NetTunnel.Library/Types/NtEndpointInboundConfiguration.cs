using ProtoBuf;

namespace NetTunnel.Library.Types
{
    [Serializable]
    [ProtoContract]
    /// <summary>
    /// The inbound endpoint contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfiguration
    {
        [ProtoMember(1)]
        public Guid PairId { get; private set; }
        [ProtoMember(2)]
        public string Name { get; private set; } = string.Empty;
        [ProtoMember(3)]
        public int Port { get; private set; }

        public NtEndpointInboundConfiguration() { }

        public NtEndpointInboundConfiguration(Guid pairId, string name, int port)
        {
            PairId = pairId;
            Name = name;
            Port = port;
        }
    }
}
