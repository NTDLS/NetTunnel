using static NetTunnel.Service.Packetizer.Constants;

namespace NetTunnel.Service.Packetizer
{
    internal class NtPacketBuffer
    {
        /// <summary>
        /// The number of bytes in the current receive buffer.
        /// </summary>
        public int ReceiveBufferUsed = 0;
        /// <summary>
        /// The current receive buffer. May be more than one packet or even a partial packet.
        /// </summary>
        public byte[] ReceiveBuffer = new byte[NtPacketDefaults.PACKET_BUFFER_SIZE];

        /// <summary>
        /// The buffer used to build a full message from the packet. This will be automatically resized if its too small.
        /// </summary>
        public byte[] PacketBuilder = new byte[NtPacketDefaults.PACKET_BUFFER_SIZE];

        /// <summary>
        /// The length of the data currently contained in the PayloadBuilder.
        /// </summary>
        public int PacketBuilderLength = 0;
    }
}
