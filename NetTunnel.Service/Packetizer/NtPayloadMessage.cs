using ProtoBuf;

namespace NetTunnel.Service.Packetizer
{
    [Serializable]
    [ProtoContract]
    internal class NtMessage
    {
        [ProtoMember(1)]
        internal DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        [ProtoMember(2)]
        public string? Label { get; set; }
        [ProtoMember(3)]
        public string? Message { get; set; }
    }
}
