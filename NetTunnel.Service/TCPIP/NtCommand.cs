using ProtoBuf;
using static NetTunnel.Service.TCPIP.Constants;

namespace NetTunnel.Service.TCPIP
{
    /// <summary>
    /// Internal command which allows for lowelevel communication betweeen server and client.
    /// </summary>
    [Serializable]
    [ProtoContract]
    internal class NtCommand
    {
        /// <summary>
        /// The enclosed message.
        /// </summary>
        [ProtoMember(1)]
        public NtMessageBase? Message { get; set; }

        /// <summary>
        /// The type of command. Tells the engine how to interpret the enclosed message.
        /// </summary>
        [ProtoMember(2)]
        public PayloadCommandType CommandType { get; set; }
    }
}
