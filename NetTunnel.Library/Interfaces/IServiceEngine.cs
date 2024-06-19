namespace NetTunnel.Library.Interfaces
{
    public interface IServiceEngine
    {
        public Logger Logger { get; }

        public void SendNotificationOfEndpointConnect(Guid connectionId, Guid tunnelId, Guid endpointId, Guid streamId);
        public void SendNotificationOfTunnelDeletion(Guid connectionId, Guid tunnelId);
        public void SendNotificationOfEndpointDataExchange(Guid connectionId, Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length);

    }
}
