using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// An inbound tunnel does not have a TCP channel, they all share the same
    ///     ServiceEngine._messageServer to send/receive and route data for the service.
    /// </summary>
    internal class TunnelInbound : TunnelBase, ITunnel
    {
        public Guid ConnectionId { get; private set; }

        public override NtDirection Direction { get => NtDirection.Inbound; }

        public bool IsLoggedIn => true; //The existence of a TunnelInbound means that is is connected. 

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
            Status = NtTunnelStatus.Established;
        }

        /// <summary>
        /// Sends a notification to the remote tunnel service containing the data that was received
        ///     by an endpoint. This data is to be sent to the endpoint connection with the matching
        ///     StreamId (which was originally sent to SendNotificationOfEndpointConnect()
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="streamId">The id that will uniquely identity the associated endpoint connections at each service</param>
        /// <param name="bytes">Bytes to be sent to endpoint connection.</param>
        /// <param name="length">Number of bytes to be sent to the endpoint connection.</param>
        public void SendNotificationOfEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid streamId, byte[] bytes, int length)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.SendNotificationOfEndpointDataExchange(ConnectionId, tunnelKey, endpointId, streamId, bytes, length);
        }

        /// <summary>
        /// Sends a notification to the remote tunnel service to let it know to connect
        ///     the associated outbound endpoint for an incoming endpoint connection.
        ///
        ///SEARCH FOR: Process:Endpoint:Connect:002: client connection is asking us to let the remote service know that a new inbound 
        ///  endpoint connection has been made and that is needs to make the associated outbound endpoint connection.
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="streamId">The id that will uniquely identity the associated endpoint connections at each service</param>
        public void SendNotificationOfEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid streamId)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.SendNotificationOfEndpointConnect(ConnectionId, tunnelKey, endpointId, streamId);
        }

        public void SendNotificationOfTunnelDeletion(DirectionalKey tunnelKey)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.SendNotificationOfTunnelDeletion(ConnectionId, tunnelKey);
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