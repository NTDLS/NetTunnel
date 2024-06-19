using NetTunnel.Library.Types;

namespace NetTunnel.Library.Interfaces
{
    public interface IServiceEngine
    {
        public ILogger Logger { get; }

        public void SendNotificationOfEndpointConnect(Guid connectionId, Guid tunnelId, Guid endpointId, Guid streamId);
        public void SendNotificationOfTunnelDeletion(Guid connectionId, DirectionalKey tunnelKey);
        public void SendNotificationOfEndpointDataExchange(Guid connectionId, Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length);
    }
}
