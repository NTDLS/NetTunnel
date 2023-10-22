using static NetTunnel.Service.Packets.Constants;

namespace NetTunnel.Service.Packets
{
    internal class NtPacketBuffer
    {
        /// <summary>
        /// The number of bytes in the current receive buffer.
        /// </summary>
        public int SingleBufferUsedLength = 0;
        /// <summary>
        /// The current receive buffer. May be more than one packet or even a partial packet.
        /// </summary>
        public byte[] SingleBuffer = new byte[Sanity.PACKET_BUFFER_SIZE];

        /// <summary>
        /// The buffer used to build a full message from the packet. This will be automatically resized if its too small.
        /// </summary>
        public byte[] PacketBuilder = new byte[Sanity.PACKET_BUFFER_SIZE];

        /// <summary>
        /// The length of the data currently contained in the PayloadBuilder.
        /// </summary>
        public int PacketBuilderLength = 0;

        public void SetSingleBuffer(byte[] bytes, int length)
        {
            if (length > SingleBuffer.Length)
            {
                Array.Resize(ref SingleBuffer, SingleBuffer.Length + length + Sanity.PACKET_BUFFER_SIZE);
            }

            Buffer.BlockCopy(bytes, 0, SingleBuffer, 0, length);

            SingleBufferUsedLength = length;
        }
    }
}
