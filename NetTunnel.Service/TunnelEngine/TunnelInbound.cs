using NetTunnel.Library.Types;

namespace NetTunnel.Service.TunnelEngine
{
    internal class TunnelInbound : TunnelBase, ITunnel
    {
        public Guid ConnectionId { get; private set; }

        public TunnelInbound(ServiceEngine core, Guid connectionId, NtTunnelConfiguration configuration)
            : base(core, configuration)
        {
            ConnectionId = connectionId;
        }

        public override void NotificationEndpointExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
        {
            //This needs to go though the _server.
            //_client.NotificationEndpointExchange(tunnelId, endpointId, streamId, bytes, length);
        }

        public override void NotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
        {
            //PROCESS:EBCONNECT.002: client connection is asking us to let the remote service know that a new inbound 
            //  endpoint connection has been made and that is needs to make the associated outbound endpoint connection.
            Core.NotificationEndpointConnect(ConnectionId, tunnelId, endpointId, streamId);


            //This needs to go though the _server.
            //_client.NotificationEndpointConnect(tunnelId, endpointId, streamId);
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }
    }
}