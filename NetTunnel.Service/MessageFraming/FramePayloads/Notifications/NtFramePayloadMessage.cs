using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadMessage : INtFramePayloadNotification
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;
    }
}
