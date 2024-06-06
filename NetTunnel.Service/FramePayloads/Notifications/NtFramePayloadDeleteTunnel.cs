using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadDeleteTunnel : IRmNotification
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
