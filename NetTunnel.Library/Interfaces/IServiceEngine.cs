using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Query;

namespace NetTunnel.Library.Interfaces
{
    public interface IServiceEngine
    {
        public void PeerNotifyOfEndpointConnect(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId);
        public void PeerNotifyOfEndpointDisconnect(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId);
        public void PeerNotifyOfTunnelDeletion(Guid connectionId, DirectionalKey tunnelKey);
        public void PeerNotifyOfEndpointDeletion(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId);
        public void PeerNotifyOfEndpointDataExchange(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length);
        public QueryUpsertEndpointReply PeerQueryUpsertEndpoint(Guid connectionId, DirectionalKey tunnelKey, EndpointConfiguration endpoint);
    }
}
