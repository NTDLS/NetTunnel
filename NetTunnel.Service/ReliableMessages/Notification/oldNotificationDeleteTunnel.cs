using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Notification
{
    [Serializable]
    [ProtoContract]
    public class oldNotificationDeleteTunnel : IRmNotification
    {
        [ProtoMember(1)]
        public Guid TunnelId { get; set; }

        public oldNotificationDeleteTunnel() { }

        public oldNotificationDeleteTunnel(Guid tunnelId)
        {
            TunnelId = tunnelId;
        }
    }
}
