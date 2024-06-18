using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Notification
{
    public class NotificationEndpointConnect : IRmNotification
    {
        public Guid StreamId { get; set; }
        public Guid TunnelId { get; set; }
        public Guid EndpointId { get; set; }

        public NotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
        {
            StreamId = streamId;
            TunnelId = tunnelId;
            EndpointId = endpointId;
        }

        public NotificationEndpointConnect()
        {
        }
    }
}
