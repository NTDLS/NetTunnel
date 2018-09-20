using System.Net.Sockets;
using NetTunnel.Library;
using NetTunnel.Library.Routing;

namespace NetTunnel.Service.Routing
{
    public class SocketState
    {
        public Route Route { get; set; }
        public SocketState Peer { get; set; }
        public int BytesReceived { get; set; }
        public Socket Socket { get; set; }
        public byte[] Buffer { get; set; }
        public byte[] PayloadBuilder;
        public int PayloadBuilderLength { get; set; }
        public int MaxBufferSize { get; set; }

        public SocketState()
        {
            Buffer = new byte[Constants.DefaultBufferSize];
            PayloadBuilder = new byte[0];
        }

        public SocketState(Socket socket, int initialBufferSize)
        {
            Socket = socket;
            Buffer = new byte[initialBufferSize];
            PayloadBuilder = new byte[0];
        }
    }
}
