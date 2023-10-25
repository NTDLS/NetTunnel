using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;

namespace NetTunnel.Service.PacketFraming
{
    internal class QueriesAwaitingReply
    {
        public Guid FrameId { get; set; }
        public AutoResetEvent WaitEvent { get; set; } = new(false);
        public INtFramePayloadReply? ReplyPayload { get; set; }
    }
}
