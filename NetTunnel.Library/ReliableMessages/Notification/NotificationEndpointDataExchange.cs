using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Notification
{
    public class NotificationEndpointDataExchange : IRmNotification
    {
        public Guid StreamId { get; set; }
        public Guid TunnelId { get; set; }
        public Guid EndpointId { get; set; }
        public byte[] Bytes { get; set; }

        public NotificationEndpointDataExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
        {
            StreamId = streamId;
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Bytes = new byte[length];

            Array.Copy(bytes, Bytes, length);
        }

        public NotificationEndpointDataExchange()
        {
            Bytes = Array.Empty<byte>();
        }
    }
}
