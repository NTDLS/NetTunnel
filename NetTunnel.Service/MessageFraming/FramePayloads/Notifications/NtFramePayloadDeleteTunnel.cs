using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadDeleteTunnel : INtFramePayloadNotification
    {
        [ProtoMember(1)]
        public Guid TunnelId { get; set; }

        public NtFramePayloadDeleteTunnel() { }

        public NtFramePayloadDeleteTunnel(Guid tunnelId)
        {
            TunnelId = tunnelId;
        }
    }
}
