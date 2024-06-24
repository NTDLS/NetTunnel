using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification
{
    public class NotifyTerminateEndpointEdgeConnection : IRmNotification
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public NotifyTerminateEndpointEdgeConnection(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            EdgeId = edgeId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }
    }
}
