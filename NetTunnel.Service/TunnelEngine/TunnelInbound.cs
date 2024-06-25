using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// An inbound tunnel does not have a TCP channel, they all share the same
    ///     ServiceEngine._messageServer to send/receive and route data for the service.
    /// </summary>
    internal class TunnelInbound : TunnelBase, ITunnel
    {
        /// <summary>
        /// The ID of the RmServer connection.This is also stored in the ServiceEngine.ServiceConnectionState.
        /// </summary>
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

        #region Interface: ITunnel.

        public void IncrementBytesSent(int bytes)
        {
            BytesSent += (ulong)bytes;
        }

        public void IncrementBytesReceived(int bytes)
        {
            BytesReceived += (ulong)bytes;
        }

        public S2SQueryUpsertEndpointReply PeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpoint)
            => ServiceEngine.PeerQueryUpsertEndpoint(ConnectionId, tunnelKey, endpoint);

        /// <summary>
        /// Sends a notification to the remote tunnel service containing the data that was received
        ///     by an endpoint. This data is to be sent to the endpoint connection with the matching
        ///     edgeId (which was originally sent to PeerNotifyOfEndpointConnect()
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        /// <param name="bytes">Bytes to be sent to endpoint connection.</param>
        /// <param name="length">Number of bytes to be sent to the endpoint connection.</param>
        public void PeerNotifyOfEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.PeerNotifyOfEndpointDataExchange(ConnectionId, tunnelKey, endpointId, edgeId, bytes, length);
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
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        public void PeerNotifyOfEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.PeerNotifyOfEndpointConnect(ConnectionId, tunnelKey, endpointId, edgeId);
        }

        public void PeerNotifyOfEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.PeerNotifyOfEndpointDisconnect(ConnectionId, tunnelKey, endpointId, edgeId);
        }

        public void PeerNotifyOfTunnelDeletion(DirectionalKey tunnelKey)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.PeerNotifyOfTunnelDeletion(ConnectionId, tunnelKey);
        }

        public void PeerNotifyOfEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
        {
            //Inbound tunnels communicate all data through the ServiceEngine._messageServer based on the ConnectionId.
            ServiceEngine.PeerNotifyOfEndpointDeletion(ConnectionId, tunnelKey, endpointId);
        }

        #endregion

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