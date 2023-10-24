using NetTunnel.Service.Packetizer.PacketPayloads;

namespace NetTunnel.Service.Packetizer
{
    internal class QueriesAwaitingReply
    {
        public Guid PacketId { get; set; }
        public AutoResetEvent WaitEvent { get; set; } = new(false);
        public IPacketPayload? ReplyPayload { get; set; }
    }
}
