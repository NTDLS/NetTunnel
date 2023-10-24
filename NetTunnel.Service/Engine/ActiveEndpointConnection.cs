using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    internal class ActiveEndpointConnection : IDisposable
    {
        public Guid StreamId { get; set; }
        public TcpClient TcpClient { get; set; }
        public Thread Thread { get; set; }
        public NetworkStream Stream { get; set; }

        public ActiveEndpointConnection(Thread thread, TcpClient tcpClient, Guid streamId)
        {
            Thread = thread;
            TcpClient = tcpClient;
            StreamId = streamId;
            Stream = tcpClient.GetStream();
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
                Stream.Dispose();
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
