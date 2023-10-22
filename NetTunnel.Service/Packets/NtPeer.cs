﻿using NetTunnel.Library.Types;

namespace NetTunnel.Service.Packets
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
