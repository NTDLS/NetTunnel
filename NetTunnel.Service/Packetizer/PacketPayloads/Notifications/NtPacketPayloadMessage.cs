using ProtoBuf;

namespace NetTunnel.Service.Packetizer.PacketPayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadMessage : IPacketPayloadNotification
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public string Message { get; set; } = string.Empty;
    }
}
