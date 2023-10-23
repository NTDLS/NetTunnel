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
    }
}
