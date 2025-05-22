using NetTunnel.Library;
using NTDLS.Helpers;
using System.Net.Sockets;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    internal class EndpointEdgeConnection : IDisposable
    {
        private readonly NetworkStream _stream;

        public bool IsConnected { get; private set; }
        public DateTime LastActivityDateTime { get; private set; } = DateTime.UtcNow;
        public DateTime StartDateTime { get; private set; } = DateTime.UtcNow;
        public Guid EdgeId { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public Thread Thread { get; private set; }
        public ulong BytesReceived { get; internal set; }
        public ulong BytesSent { get; internal set; }
        public PacketSequenceBuffer SequenceBuffer { get; internal set; } = new();

        public double ActivityAgeInMilliseconds
            => (DateTime.UtcNow - LastActivityDateTime).TotalMilliseconds;

        public double StartAgeInMilliseconds
            => (DateTime.UtcNow - StartDateTime).TotalMilliseconds;

        public EndpointEdgeConnection(Thread thread, TcpClient tcpClient, Guid edgeId)
        {
            Thread = thread;
            TcpClient = tcpClient;
            EdgeId = edgeId;
            _stream = tcpClient.GetStream();
            IsConnected = true;
        }

        public void Disconnect()
        {
            Exceptions.Ignore(_stream.Close);
            Exceptions.Ignore(TcpClient.Close);
            IsConnected = false;
        }

        public void Write(long packetSequence, byte[] buffer)
        {
            SequenceBuffer.Consume(_stream, packetSequence, buffer);

            BytesSent += (ulong)buffer.Length;
            LastActivityDateTime = DateTime.UtcNow;
        }

        public bool Read(ref PumpBuffer buffer)
        {
            LastActivityDateTime = DateTime.UtcNow;
            buffer.Length = _stream.Read(buffer.Bytes, 0, buffer.Bytes.Length);
            BytesReceived += (ulong)buffer.Length;
            return buffer.Length > 0;
        }

        public void Dispose()
        {
            Disconnect();

            Exceptions.Ignore(_stream.Dispose);
            Exceptions.Ignore(TcpClient.Dispose);
        }
    }
}
