using NetTunnel.Service.MessageFraming;
using NetTunnel.Service.MessageFraming.FramePayloads.Notifications;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.MessageFraming.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine.Endpoints;

namespace NetTunnel.Service.TunnelEngine.Tunnels
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

        public void Start();
        public void Stop();

        public Task<T?> SendStreamFramePayloadQuery<T>(INtFramePayloadQuery payload);

        /// <summary>
        /// Sends a reply to a INtFramePayloadQuery
        /// </summary>
        public void SendStreamFramePayloadReply(NtFrame queryFrame, INtFramePayloadReply payload);

        /// <summary>
        /// Sends a one way (fire and forget) INtFramePayloadNotification.
        /// </summary>
        public void SendStreamFrameNotification(INtFramePayloadNotification payload);

        internal List<IEndpoint> Endpoints { get; set; }

        public void ApplyQueryReply(Guid frameId, INtFramePayloadReply replyPayload);

        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; }
        public ulong CurrentConnections { get; }
    }
}
