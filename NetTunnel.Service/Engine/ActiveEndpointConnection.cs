using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    internal class ActiveEndpointConnection : IDisposable
    {
        public DateTime StartDateTime { get; private set; } = DateTime.UtcNow;
        public DateTime LastActivityDateTime { get; private set; } = DateTime.UtcNow;
        public Guid StreamId { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public Thread Thread { get; private set; }
        public bool IsConnected { get; private set; }

        private NetworkStream _stream;

        public ActiveEndpointConnection(Thread thread, TcpClient tcpClient, Guid streamId)
        {
            Thread = thread;
            TcpClient = tcpClient;
            StreamId = streamId;
            _stream = tcpClient.GetStream();
            IsConnected = true;
        }

        public void Disconnect()
        {
            try { _stream.Close(); } catch { }
            try { TcpClient.Close(); } catch { }
            IsConnected = false;
        }

        public void Write(byte[] buffer)
        {
            LastActivityDateTime = DateTime.UtcNow;
            _stream.Write(buffer);
        }

        public bool Read(ref byte[] buffer)
        {
            LastActivityDateTime = DateTime.UtcNow;
            return _stream.Read(buffer, 0, buffer.Length) > 0;
        }

        public void Dispose()
        {
            Disconnect();

            try { _stream.Dispose(); } catch { }
            try { TcpClient.Dispose(); } catch { }
        }
    }
}
