using System.Net.Sockets;

namespace NetTunnel.Service.TCPIP
{
    internal class NtPeer
    {
        public NtPeer(Socket socket)
        {
            Socket = socket;
        }

        public Socket Socket { get; private set; }
        public NtPacket Packet { get; internal set; } = new NtPacket();
    }
}
