using ProtoBuf;

namespace NetTunnel.Service.Packetizer.PacketPayloads.Replies
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadBoolean : IPacketPayloadReply
    {
        [ProtoMember(1)]
        public bool Value { get; set; }

        public NtPacketPayloadBoolean()
        {
        }

        public NtPacketPayloadBoolean(bool value)
        {
            Value = value;
        }
    }
}
