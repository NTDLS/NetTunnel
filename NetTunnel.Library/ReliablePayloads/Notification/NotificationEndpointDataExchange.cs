using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification
{
    public class NotificationEndpointDataExchange : IRmNotification
    {
        public Guid StreamId { get; set; }
        public DirectionalKey? TunnelKey { get; set; }
        public Guid EndpointId { get; set; }
        public byte[] Bytes { get; set; }

        public NotificationEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid streamId, byte[] bytes, int length)
        {
            StreamId = streamId;
            TunnelKey = tunnelKey;
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
