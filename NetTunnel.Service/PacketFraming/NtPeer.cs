using NetTunnel.Service.Types;

namespace NetTunnel.Service.PacketFraming
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
