using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadDeleteTunnel : INtFramePayloadQuery
    {
        [ProtoMember(1)]
        public Guid TunnelPairId { get; set; }

        public NtFramePayloadDeleteTunnel() { }

        public NtFramePayloadDeleteTunnel(Guid tunnelPairId)
        {
            TunnelPairId = tunnelPairId;
        }
    }
}
