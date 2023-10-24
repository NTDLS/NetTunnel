using ProtoBuf;

namespace NetTunnel.Service.Packetizer
{
    /// <summary>
    /// Internal packet which allows for lowelevel communication betweeen server and client.
    /// </summary>
    [Serializable]
    [ProtoContract]
    internal class NtPacket
    {
        [ProtoMember(1)]
        public string EnclosedPayloadType { get; set; } = string.Empty;
        [ProtoMember(2)]
        public byte[] Payload { get; set; } = Array.Empty<byte>();
        [ProtoMember(3)]
        public bool IsQuery { get; set; } = false;
        [ProtoMember(4)]
        public bool IsReply { get; set; } = false;

        [ProtoMember(5)]
        public Guid PacketId { get; set; } = Guid.NewGuid();
    }
}
