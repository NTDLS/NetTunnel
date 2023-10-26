using NetTunnel.Service.MessageFraming;
using NetTunnel.Service.MessageFraming.FramePayloads.Notifications;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.MessageFraming.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine;

namespace NetTunnel.Service.Types
{
    internal interface ITunnel
    {
        public TunnelEngineCore Core { get; }

        public bool KeepRunning { get; }
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service tunnel has the same id.
        /// </summary>
        public Guid PairId { get; }
        public string Name { get; }

        public Task<T?> SendStreamFramePayloadQuery<T>(INtFramePayloadQuery payload);

        /// <summary>
        /// Sends a reply to a INtFramePayloadQuery
        /// </summary>
        public void SendStreamFramePayloadReply(NtFrame queryFrame, INtFramePayloadReply payload);

        /// <summary>
        /// Sends a one way (fire and forget) INtFramePayloadNotification.
        /// </summary>
        public void SendStreamFrameNotification(INtFramePayloadNotification payload);

        public void ApplyQueryReply(Guid frameId, INtFramePayloadReply replyPayload);
    }
}
