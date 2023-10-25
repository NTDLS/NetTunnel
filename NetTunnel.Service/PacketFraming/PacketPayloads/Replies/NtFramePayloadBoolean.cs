using ProtoBuf;

namespace NetTunnel.Service.PacketFraming.PacketPayloads.Replies
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
