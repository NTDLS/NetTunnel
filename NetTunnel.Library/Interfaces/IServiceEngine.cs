using NetTunnel.Library.Payloads;

namespace NetTunnel.Library.Interfaces
{
    public interface IServiceEngine
    {
        public ILogger Logger { get; }

        public void PeerNotifyOfEndpointConnect(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid streamId);
        public void PeerNotifyOfTunnelDeletion(Guid connectionId, DirectionalKey tunnelKey);
        public void PeerNotifyOfEndpointDeletion(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId);
        public void PeerNotifyOfEndpointDataExchange(Guid connectionId, DirectionalKey tunnelKey, Guid endpointId, Guid streamId, byte[] bytes, int length);
    }
}
