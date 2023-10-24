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

        public Task<T?> SendStreamPacketPayloadQuery<T>(IPacketPayloadQuery payload);

        /// <summary>
        /// Sends a reply to a IPacketPayloadQuery
        /// </summary>
        public void SendStreamPacketPayloadReply(NtPacket queryPacket, IPacketPayloadReply payload);

        /// <summary>
        /// Sends a one way (fire and forget) IPacketPayloadNotification.
        /// </summary>
        public void SendStreamPacketNotification(IPacketPayloadNotification payload);

        public void ApplyQueryReply(Guid packetId, IPacketPayloadReply replyPayload);
    }
}
