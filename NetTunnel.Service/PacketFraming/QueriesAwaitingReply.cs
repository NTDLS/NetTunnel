using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;

namespace NetTunnel.Service.PacketFraming
{
    internal class QueriesAwaitingReply
    {
        public Guid PacketId { get; set; }
        public AutoResetEvent WaitEvent { get; set; } = new(false);
        public IPacketPayloadReply? ReplyPayload { get; set; }
    }
}
