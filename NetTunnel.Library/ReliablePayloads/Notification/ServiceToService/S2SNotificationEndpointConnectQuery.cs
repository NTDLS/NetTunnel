using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification.ServiceToService
{
    public class S2SNotificationEndpointConnectQuery : IRmQuery<S2SNotificationEndpointConnectQueryReply>
    {
        public Guid EdgeId { get; set; }
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }
        /// <summary>
        /// The application protocol (ALPN) the client negotiated with the inbound endpoint ("h2" or "http/1.1"), so the outbound endpoint can ask the destination for the same one. Null when TLS was not terminated.
        /// </summary>
        public string? ApplicationProtocol { get; set; }

        public S2SNotificationEndpointConnectQuery(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, string? applicationProtocol = null)
        {
            EdgeId = edgeId;
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
            ApplicationProtocol = applicationProtocol;
        }
    }

    public class S2SNotificationEndpointConnectQueryReply : IRmQueryReply
    {
    }
}
