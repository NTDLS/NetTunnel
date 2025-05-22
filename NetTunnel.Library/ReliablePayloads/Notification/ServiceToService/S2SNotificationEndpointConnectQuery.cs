using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.ServiceToService
{
    public class S2SNotificationEndpointConnectQuery : IRmQuery<S2SNotificationEndpointConnectQueryReply>
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public S2SNotificationEndpointConnectQuery(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            EdgeId = edgeId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }
    }

    public class S2SNotificationEndpointConnectQueryReply : IRmQueryReply
    {
    }
}
