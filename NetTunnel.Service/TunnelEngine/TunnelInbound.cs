using NetTunnel.Library.Types;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// An inbound tunnel does not have a TCP channel, they all share the same
    ///     ServiceEngine._messageServer to send/receive and route data for the service.
    /// </summary>
    internal class TunnelInbound : TunnelBase, ITunnel
    {
        public Guid ConnectionId { get; private set; }

        /// <summary>
        /// When a connection comes in and registers a tunnel, we create a new instance of TunnelInbound
        ///     with the connectionId of the connection that requested the tunnel registration.
        ///     
        ///     That ConnectionId is the id that we use to communicate back to the remove tunnel service.
        /// </summary>
        /// <param name="serviceEngine"></param>
        /// <param name="connectionId"></param>
        /// <param name="configuration"></param>
        public TunnelInbound(ServiceEngine serviceEngine, Guid connectionId, TunnelConfiguration configuration)
            : base(serviceEngine, configuration)
        {
            ConnectionId = connectionId;
        }

        public override void SendNotificationOfEndpointDataExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.SendNotificationOfEndpointDataExchange(ConnectionId, tunnelId, endpointId, streamId, bytes, length);
        }

        /// <summary>
        ///SEARCH FOR: Process:Endpoint:Connect:002: client connection is asking us to let the remote service know that a new inbound 
        ///  endpoint connection has been made and that is needs to make the associated outbound endpoint connection.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        /// <param name="streamId">The ID of the remote stream. This is how we identify which actual endpoint connection this connection is paired to.</param>
        public override void SendNotificationOfEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.SendNotificationOfEndpointConnect(ConnectionId, tunnelId, endpointId, streamId);
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