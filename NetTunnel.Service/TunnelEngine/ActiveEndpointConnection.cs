using NetTunnel.Library;
using System.Net.Sockets;

namespace NetTunnel.Service.TunnelEngine
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

        public double ActivityAgeInMiliseconds => (DateTime.UtcNow - LastActivityDateTime).TotalMilliseconds;
        public double StartAgeInMiliseconds => (DateTime.UtcNow - StartDateTime).TotalMilliseconds;

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
            Utility.TryAndIgnore(_stream.Close);
            Utility.TryAndIgnore(TcpClient.Close);
            IsConnected = false;
        }

        public void Write(byte[] buffer)
        {
            LastActivityDateTime = DateTime.UtcNow;
            _stream.Write(buffer);
        }

        public bool Read(ref byte[] buffer, out int length)
        {
            LastActivityDateTime = DateTime.UtcNow;
            length = _stream.Read(buffer, 0, buffer.Length);
            return length > 0;
        }

        public void Dispose()
        {
            Disconnect();

            Utility.TryAndIgnore(_stream.Dispose);
            Utility.TryAndIgnore(TcpClient.Dispose);
        }
    }
}
