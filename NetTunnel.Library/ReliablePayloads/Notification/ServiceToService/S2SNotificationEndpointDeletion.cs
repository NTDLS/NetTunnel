using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.ServiceToService
{
    public class S2SNotificationEndpointDeletion : IRmNotification
    {
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public S2SNotificationEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
        {
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }
    }
}
