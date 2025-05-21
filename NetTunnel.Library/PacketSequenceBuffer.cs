namespace NetTunnel.Library
{
    public class PacketSequenceBuffer
    {
        public Dictionary<long, byte[]> Buffer { get; set; } = new();

        private long _lastConsumedPacketSequence = -1;

        public void WriteToStream(Stream stream, long sequence, byte[] data)
        {
            lock (Buffer)
            {
                //The next packet in the sequence is the next one that needs to be sent. Flush it to the stream.
                if (_lastConsumedPacketSequence + 1 == sequence)
                {
                    _lastConsumedPacketSequence = sequence;
                    stream.Write(data);
                    return;
                }

                //We received out-of-order packets. Store them in the buffer.
                Buffer.Add(sequence, data);

                //Flush any packets that are now in order.
                while (Buffer.TryGetValue(_lastConsumedPacketSequence + 1, out var bytes))
                {
                    Buffer.Remove(_lastConsumedPacketSequence + 1);
                    _lastConsumedPacketSequence++;
                    stream.Write(bytes);
                }
            }
        }
    }
}
