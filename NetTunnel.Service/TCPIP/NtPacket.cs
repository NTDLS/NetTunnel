using static NetTunnel.Service.TCPIP.Constants;

namespace NetTunnel.Service.TCPIP
{
    internal class NtPacket
    {
        /// <summary>
        /// The number of bytes in the current receive buffer.
        /// </summary>
        public int BufferLength = 0;
        /// <summary>
        /// The current receive buffer. May be more than one packet or even a partial packet.
        /// </summary>
        public byte[] Buffer = new byte[Sanity.PACKET_BUFFER_SIZE];

        /// <summary>
        /// The buffer used to build a full message from the packet. This will be automatically resized if its too small.
        /// </summary>
        public byte[] PayloadBuilder = new byte[Sanity.PACKET_BUFFER_SIZE];

        /// <summary>
        /// The length of the data currently contained in the PayloadBuilder.
        /// </summary>
        public int PayloadBuilderLength = 0;
    }
}
