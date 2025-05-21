using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.ServiceToService
{
    public class S2SNotificationEndpointDataExchange : IRmNotification
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }
        public byte[] Bytes { get; set; }
        public long PacketSequence { get; set; }

        public S2SNotificationEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, long packetSequence, byte[] bytes, int length)
        {
            EdgeId = edgeId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
            PacketSequence = packetSequence;
            Bytes = new byte[length];

            Array.Copy(bytes, Bytes, length);
        }

        public S2SNotificationEndpointDataExchange()
        {
            Bytes = Array.Empty<byte>();
            TunnelKey = new DirectionalKey();
        }
    }
}
