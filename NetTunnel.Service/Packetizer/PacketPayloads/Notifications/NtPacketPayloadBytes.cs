using ProtoBuf;

namespace NetTunnel.Service.Packetizer.PacketPayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadBytes : IPacketPayloadNotification
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
    }
}
