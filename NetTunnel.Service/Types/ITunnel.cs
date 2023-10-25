using NetTunnel.Service.Engine;
using NetTunnel.Service.PacketFraming;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.PacketFraming.PacketPayloads.Queries;
using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;

namespace NetTunnel.Service.Types
{
    internal interface ITunnel
    {
        public EngineCore Core { get; }

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
