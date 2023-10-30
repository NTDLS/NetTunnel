using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Replies
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadBoolean : INtFramePayloadReply
    {
        [ProtoMember(1)]
        public bool Value { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;

        public NtFramePayloadBoolean()
        {
        }

        public NtFramePayloadBoolean(bool value)
        {
            Value = value;
        }

        public NtFramePayloadBoolean(Exception exception)
        {
            Value = false;
            Message = exception.Message;
        }
    }
}
