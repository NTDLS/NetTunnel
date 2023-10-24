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
        public Guid Id { get; set; } = Guid.NewGuid();

        [ProtoMember(2)]
        public string EnclosedPayloadType { get; set; } = string.Empty;

        [ProtoMember(3)]
        public byte[] Payload { get; set; } = Array.Empty<byte>();

    }
}
