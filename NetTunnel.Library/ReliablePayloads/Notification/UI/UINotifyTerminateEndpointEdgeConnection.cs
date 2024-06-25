using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.UI
{
    public class UINotifyTerminateEndpointEdgeConnection : IRmNotification
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public UINotifyTerminateEndpointEdgeConnection(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            EdgeId = edgeId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }
    }
}
