using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.ServiceToService
{
    public class S2SNotificationEndpointDisconnect : IRmNotification
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public S2SNotificationEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            EdgeId = edgeId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }

        public S2SNotificationEndpointDisconnect()
        {
            TunnelKey = new DirectionalKey();
        }
    }
}
