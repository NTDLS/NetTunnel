using ProtoBuf;

namespace NetTunnel.Service.Packetizer.PacketPayloads
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadBoolean : IPacketPayload
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
