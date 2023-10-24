using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    internal class ActiveEndpointConnection : IDisposable
    {
        public Guid StreamId { get; set; }
        public TcpClient TcpClient { get; set; }
        public Thread Thread { get; set; }
        private NetworkStream _stream { get; set; }

        public ActiveEndpointConnection(Thread thread, TcpClient tcpClient, Guid streamId)
        {
            Thread = thread;
            TcpClient = tcpClient;
            StreamId = streamId;
            _stream = tcpClient.GetStream();
        }

        public void Write(byte[] buffer)
        {
            _stream.Write(buffer);
        }

        public bool Read(ref byte[] buffer)
        {
            return _stream.Read(buffer, 0, buffer.Length) > 0;
        }

        public void Dispose()
        {
            try
            {
                TcpClient.Close();
            }
            catch { }

            try
            {
                _stream.Dispose();
            }
            catch { }

            try
            {
                TcpClient.Dispose();
            }
            catch { }
        }
    }
}
