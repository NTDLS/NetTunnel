using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadDeleteTunnel : INtFramePayloadNotification
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
