using ProtoBuf;
using static NetTunnel.Service.Packetizer.Constants;

namespace NetTunnel.Service.Packetizer
{
    /// <summary>
    /// Internal packet which allows for lowelevel communication betweeen server and client.
    /// </summary>
    [Serializable]
    [ProtoContract]
    internal class NtPacket
    {
        /// <summary>
        /// The enclosed message.
        /// </summary>
        [ProtoMember(1)]
        public NtMessage? Message { get; set; }

        /// <summary>
        /// The type of command. Tells the engine how to interpret the enclosed message.
        /// </summary>
        [ProtoMember(2)]
        public NtPacketType PacketType { get; set; }
    }
}
