using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification
{
    public class NotificationEndpointConnect : IRmNotification
    {
        public Guid StreamId { get; set; }
        public DirectionalKey? TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public NotificationEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid streamId)
        {
            StreamId = streamId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }

        public NotificationEndpointConnect()
        {
        }
    }
}
