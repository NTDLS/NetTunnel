using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Interfaces
{
    public interface IServiceEngine
    {
        public void S2SPeerNotificationEndpointConnect(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId);
        public void S2SPeerNotificationEndpointDisconnect(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId);
        public void S2SPeerNotificationTunnelDeletion(Guid connectionId, DirectionalKey tunnelKey);
        public void S2SPeerNotificationEndpointDeletion(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId);
        public void S2SPeerNotificationEndpointDataExchange(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length);
        public S2SQueryUpsertEndpointReply S2SPeerQueryUpsertEndpoint(Guid connectionId, DirectionalKey tunnelKey, EndpointConfiguration endpoint);
        public void UINotifyLog(Guid connectionId, DateTime timestamp, NtLogSeverity severity, string text);
    }
}
