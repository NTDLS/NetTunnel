using ProtoBuf;

namespace NetTunnel.Service.Packetizer
{
    [Serializable]
    [ProtoContract]
    internal class NtPayload
    {
        [ProtoMember(1)]
        public string? EnclosedType { get; set; }
        /// <summary>
        /// TODO: This should either be an object or json.
        /// </summary>
        [ProtoMember(2)]
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
}
