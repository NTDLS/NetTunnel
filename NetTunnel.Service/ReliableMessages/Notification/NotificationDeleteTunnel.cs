using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Notification
{
    [Serializable]
    [ProtoContract]
    public class NotificationDeleteTunnel : IRmNotification
    {
        [ProtoMember(1)]
        public Guid TunnelId { get; set; }

        public NotificationDeleteTunnel() { }

        public NotificationDeleteTunnel(Guid tunnelId)
        {
            TunnelId = tunnelId;
        }
    }
}
