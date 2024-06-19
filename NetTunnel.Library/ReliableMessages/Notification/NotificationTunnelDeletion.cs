using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Notification
{
    public class NotificationTunnelDeletion : IRmNotification
    {
        public DirectionalKey? TunnelKey { get; set; }

        public NotificationTunnelDeletion(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }

        public NotificationTunnelDeletion()
        {
        }
    }
}
