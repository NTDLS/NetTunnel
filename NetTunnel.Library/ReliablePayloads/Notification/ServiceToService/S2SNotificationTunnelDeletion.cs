using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.ServiceToService
{
    public class S2SNotificationTunnelDeletion : IRmNotification
    {
        public DirectionalKey TunnelKey { get; set; }

        public S2SNotificationTunnelDeletion(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }
}
