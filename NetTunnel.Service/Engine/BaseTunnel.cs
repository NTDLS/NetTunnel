using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    internal class BaseTunnel
    {
        protected bool _keepRunning = false;
        protected NetworkStream? _stream;

    }
}
