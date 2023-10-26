using NetTunnel.Service.MessageFraming.FramePayloads.Replies;

namespace NetTunnel.Service.MessageFraming
{
    internal class QueriesAwaitingReply
    {
        public Guid FrameId { get; set; }
        public AutoResetEvent WaitEvent { get; set; } = new(false);
        public INtFramePayloadReply? ReplyPayload { get; set; }
    }
}
