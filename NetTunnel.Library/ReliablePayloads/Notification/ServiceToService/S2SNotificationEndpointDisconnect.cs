using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.ServiceToService
{
    public class S2SNotificationEndpointDisconnect : IRmNotification
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }
        /// <summary>
        /// How many data packets the sender sent on the edge before this notification. Notifications are handled concurrently and out of
        ///     order, so the receiver must deliver this many packets before it closes the edge. Negative means unknown: close at once.
        /// </summary>
        public long PacketCount { get; set; } = -1;

        public S2SNotificationEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, long packetCount = -1)
        {
            EdgeId = edgeId;
            PacketCount = packetCount;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }

        public S2SNotificationEndpointDisconnect()
        {
            TunnelKey = new DirectionalKey();
        }
    }
}
