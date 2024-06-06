using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadMessage : IRmNotification
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;
    }
}
