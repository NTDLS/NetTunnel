using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification
{
    public class NotificationEndpointDeletion : IRmNotification
    {
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public NotificationEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
        {
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }
    }
}
