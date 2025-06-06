﻿using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Interfaces
{
    public interface ITunnel
    {
        public double? PingTime { get; set; }
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

        public IEndpoint UpsertEndpoint(EndpointConfiguration configuration, string? username);

        /// <summary>
        /// Disconnect the endpoint edge from the external server, browser, etc.
        /// </summary>
        public void DeleteEndpoint(Guid endpointId, string? username);
        public void DisconnectEndpointEdge(Guid endpointId, Guid edgeId);

        public TunnelConfiguration CloneConfiguration();

        /// <summary>
        /// Sends a notification to the remote tunnel service containing the data that was received
        ///     by an endpoint. This data is to be sent to the endpoint connection with the matching
        ///     edgeId (which was originally sent to S2SPeerNotificationEndpointConnect()
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        /// <param name="packetSequence">The sequence number of the packet for outbound buffering and ordering outbound edge data.</param>
        /// <param name="bytes">Bytes to be sent to endpoint connection.</param>
        /// <param name="length">Number of bytes to be sent to the endpoint connection.</param>
        public void S2SPeerNotificationEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, long packetSequence, byte[] bytes, int length);

        /// <summary>
        /// Sends a notification to the remote tunnel service to let it know to connect
        ///     the associated outbound endpoint for an incoming endpoint connection.
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        public void S2SPeerNotificationEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId);
        public void S2SPeerNotificationEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId);
        public void S2SPeerNotificationTunnelDeletion(DirectionalKey tunnelKey);
        public void S2SPeerNotificationEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId);
        public S2SQueryUpsertEndpointReply S2SPeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpointId);

        public void LoadEndpoints(List<EndpointConfiguration> endpoints);
        public void IncrementBytesSent(int bytes);
        public void IncrementBytesReceived(int bytes);
        public TunnelPropertiesDisplay GetProperties();
        public EndpointPropertiesDisplay GetEndpointProperties(DirectionalKey endpointKey);
        public List<EndpointEdgeConnectionDisplay> GetEndpointEdgeConnections(DirectionalKey endpointKey);
        public List<EndpointConfiguration> GetEndpointsForDisplay();
    }
}
