using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Replies
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadBoolean : INtFramePayloadReply
    {
        [ProtoMember(1)]
        public bool Value { get; set; }

        public NtFramePayloadBoolean()
        {
        }

        public NtFramePayloadBoolean(bool value)
        {
            Value = value;
        }
    }
}
