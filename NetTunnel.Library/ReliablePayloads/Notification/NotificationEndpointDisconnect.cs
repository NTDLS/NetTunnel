using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Notification
{
    public class NotificationEndpointDisconnect : IRmNotification
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public NotificationEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            EdgeId = edgeId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }

        public NotificationEndpointDisconnect()
        {
            TunnelKey = new DirectionalKey();
        }
    }
}
