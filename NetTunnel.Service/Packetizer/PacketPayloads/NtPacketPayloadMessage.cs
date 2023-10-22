using ProtoBuf;

namespace NetTunnel.Service.Packetizer.PacketPayloads
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadMessage : IPacketPayload
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;
    }
}
