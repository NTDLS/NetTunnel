using NetTunnel.Service.TunnelEngine.Tunnels;

namespace NetTunnel.Service.MessageFraming
{
    internal class NtPeer
    {
        public NtPeer(ITunnel tunnel)
        {
            Tunnel = tunnel;
        }

        public ITunnel Tunnel { get; private set; }
    }
}
