using static NetTunnel.Service.PacketFraming.Types;

namespace NetTunnel.Service.PacketFraming
{
    internal class NtFrameBuffer
    {
        /// <summary>
        /// The number of bytes in the current receive buffer.
        /// </summary>
        public int ReceiveBufferUsed = 0;
        /// <summary>
        /// The current receive buffer. May be more than one frame or even a partial frame.
        /// </summary>
        public byte[] ReceiveBuffer = new byte[NtFrameDefaults.FRAME_BUFFER_SIZE];

        /// <summary>
        /// The buffer used to build a full message from the frame. This will be automatically resized if its too small.
        /// </summary>
        public byte[] FrameBuilder = new byte[NtFrameDefaults.FRAME_BUFFER_SIZE];

        /// <summary>
        /// The length of the data currently contained in the PayloadBuilder.
        /// </summary>
        public int FrameBuilderLength = 0;
    }
}
