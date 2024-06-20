using NetTunnel.Library.Payloads;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Interfaces
{
    public interface ITunnel
    {
        public TunnelConfiguration Configuration { get; }
        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; set; }
        public ulong CurrentConnections { get; set; }
        public IServiceEngine ServiceEngine { get; }
        public List<IEndpoint> Endpoints { get; }
        public NtDirection Direction { get; }
        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey TunnelKey { get; }

        /// <summary>
        /// Lets us know if the tunnel is connected to another service endpoint.
        /// </summary>
        public bool IsLoggedIn { get; }

        public void Start();
        public void Stop();

        public IEndpoint UpsertEndpoint(EndpointConfiguration configuration);
        public void DeleteEndpoint(Guid endpointId);
        public TunnelConfiguration CloneConfiguration();

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
        public void SendNotificationOfEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid streamId, byte[] bytes, int length);

        /// <summary>
        /// Sends a notification to the remote tunnel service to let it know to connect
        ///     the associated outbound endpoint for an incoming endpoint connection.
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="streamId">The id that will uniquely identity the associated endpoint connections at each service</param>
        public void SendNotificationOfEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid streamId);

        public void SendNotificationOfTunnelDeletion(DirectionalKey tunnelKey);
        public void SendNotificationOfEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId);
    }
}
