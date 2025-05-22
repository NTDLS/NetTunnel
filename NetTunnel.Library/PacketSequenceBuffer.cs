namespace NetTunnel.Library
{
    public class PacketSequenceBuffer
    {
        private readonly Dictionary<long, byte[]> _buffer = new();

        private long _lastConsumedSequence = -1;

        public void WriteToStream(Stream stream, long sequence, byte[] data)
        {
            lock (_buffer)
            {
                //The next packet in the sequence is the next one that needs to be sent. Flush it to the stream.
                if (_lastConsumedSequence + 1 == sequence)
                {
                    _lastConsumedSequence = sequence;
                    stream.Write(data);
                }
                else
                {
                    //We received out-of-order packets. Store them in the buffer.
                    _buffer.Add(sequence, data);
                }

                //Flush any packets that are now in order.
                while (_buffer.TryGetValue(_lastConsumedSequence + 1, out var bytes))
                {
                    _buffer.Remove(_lastConsumedSequence + 1);
                    _lastConsumedSequence++;
                    stream.Write(bytes);
                }
            }
        }
    }
}
