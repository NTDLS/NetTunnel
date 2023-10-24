using NetTunnel.Library.Types;
using NetTunnel.Service.Types;

namespace NetTunnel.Service.Packetizer
{
    internal class NtPeer
    {
        public NtPeer(ITunnel tunnel)
        {
            Tunnel = tunnel;
        }

        public ITunnel Tunnel { get; private set; }
        public NtPacketBuffer Packet { get; internal set; } = new NtPacketBuffer();
    }
}
