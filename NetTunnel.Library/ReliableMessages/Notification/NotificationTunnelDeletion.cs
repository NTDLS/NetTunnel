using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Notification
{
    public class NotificationTunnelDeletion : IRmNotification
    {
        public Guid TunnelId { get; set; }

        public NotificationTunnelDeletion(Guid tunnelId)
        {
            TunnelId = tunnelId;
        }

        public NotificationTunnelDeletion()
        {
        }
    }
}
