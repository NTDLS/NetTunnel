using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.Helpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening tunnel service.
    /// </summary>
    internal class TunnelBase : ITunnel
    {
        /// <summary>
        /// Returns Outbound if the tunnel is owned by the local service, otherwise returns Inbound.
        /// </summary>
        public virtual NtDirection Direction { get => throw new NotImplementedException("This function should be overridden."); }
        public DirectionalKey TunnelKey => new(Configuration.TunnelId, Direction);

        public int ChangeHash =>
            Configuration.TunnelId.GetHashCode()
            + Configuration.Name.GetHashCode();

        #region Public Properties.

        public double? PingTime { get; set; }
        public TunnelConfiguration Configuration { get; private set; }
        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; set; }
        public ulong CurrentConnections { get; set; }
        public IServiceEngine ServiceEngine { get; private set; }
        public bool KeepRunning { get; private set; } = false;
        private readonly object _endpointsLock = new();
        private List<IEndpoint> _endpoints = new();

        /// <summary>
        /// A snapshot of the endpoints; safe to enumerate while endpoints are being added or removed.
        /// </summary>
        public IReadOnlyList<IEndpoint> Endpoints
        {
            get
            {
                lock (_endpointsLock)
                {
                    return _endpoints.ToList();
                }
            }
        }
        bool ITunnel.IsLoggedIn => throw new NotImplementedException("This function should be overridden.");

        #endregion

        public TunnelBase(ServiceEngine serviceEngine, TunnelConfiguration configuration)
        {
            ServiceEngine = serviceEngine;
            Configuration = configuration.CloneConfiguration();
        }

        #region Interface: ITunnel.

        public void LoadEndpoints(List<EndpointConfiguration> endpoints)
        {
            var tunnelDirection = this is TunnelInbound ? NtDirection.Inbound : NtDirection.Outbound;

            Singletons.Logger.Verbose($"Loading endpoints for {tunnelDirection} tunnel '{Configuration.Name}'.");

            var newEndpoints = new List<IEndpoint>();

            endpoints.Where(o => o.Direction == NtDirection.Inbound)
                .ToList().ForEach(o => newEndpoints.Add(new EndpointInbound(ServiceEngine, this, o)));

            endpoints.Where(o => o.Direction == NtDirection.Outbound)
                .ToList().ForEach(o => newEndpoints.Add(new EndpointOutbound(ServiceEngine, this, o)));

            List<IEndpoint> oldEndpoints;
            lock (_endpointsLock)
            {
                oldEndpoints = _endpoints;
                _endpoints = newEndpoints;
            }

            //Stop any endpoints from a previous connection, otherwise they keep running orphaned
            //  and the inbound listeners hold their ports so that the new ones cannot bind.
            foreach (var endpoint in oldEndpoints)
            {
                Exceptions.Ignore(endpoint.Stop);
            }

            Singletons.Logger.Verbose($"Starting endpoints for {tunnelDirection} tunnel '{Configuration.Name}'.");

            foreach (var endpoint in newEndpoints)
            {
                endpoint.Start();
            }
        }

        public List<EndpointConfiguration> GetEndpointsForDisplay()
        {
            var results = new List<EndpointConfiguration>();

            foreach (var endpoint in Endpoints)
            {
                results.Add(endpoint.GetForDisplay());
            }

            return results;
        }

        void ITunnel.IncrementBytesSent(int bytes)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.IncrementBytesReceived(int bytes)
           => throw new NotImplementedException("This function should be overridden.");

        S2SQueryUpsertEndpointReply ITunnel.S2SPeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpointId)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.S2SPeerNotificationEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, long sequence, byte[] bytes, int length)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.S2SPeerNotificationEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.S2SPeerNotificationTunnelDeletion(DirectionalKey tunnelKey)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.S2SPeerNotificationEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.S2SPeerNotificationEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
           => throw new NotImplementedException("This function should be overridden.");

        public EndpointPropertiesDisplay GetEndpointProperties(DirectionalKey endpointKey)
            => Endpoints.Single(o => o.EndpointKey == endpointKey).GetProperties();

        public List<EndpointEdgeConnectionDisplay> GetEndpointEdgeConnections(DirectionalKey endpointKey)
            => Endpoints.Single(o => o.EndpointKey == endpointKey).GetEdgeConnections();

        public TunnelPropertiesDisplay GetProperties()
        {
            //There is no connection state while the tunnel is disconnected.
            var serviceConnectionState = Singletons.ServiceEngine
                .ServiceConnectionStates.Use(o => o.Values.FirstOrDefault(o => o.TunnelKey == TunnelKey));

            var prop = new TunnelPropertiesDisplay()
            {
                KeyHash = serviceConnectionState?.KeyHash ?? string.Empty,
                KeyLength = serviceConnectionState?.KeyLength ?? 0,
                BytesReceived = BytesReceived,
                BytesSent = BytesSent,
                CurrentConnections = CurrentConnections,
                Direction = Direction,
                PingTime = PingTime,
                Status = Status,
                TotalConnections = TotalConnections,
                TunnelKey = TunnelKey,
                RemoteClientAddress = serviceConnectionState?.RemoteClientAddress ?? string.Empty,
                LocalClientAddress = serviceConnectionState?.LocalClientAddress ?? string.Empty,
                IsAuthenticated = serviceConnectionState?.IsAuthenticated ?? false,
                KeepRunning = KeepRunning,
                LoginTime = serviceConnectionState?.LoginTime ?? DateTime.MinValue,
                IsKeyExchangeComplete = serviceConnectionState?.IsKeyExchangeComplete ?? false,
                LoggedInUserName = serviceConnectionState?.UserName ?? string.Empty,
                ServiceId = Configuration.ServiceId,
                Name = Configuration.Name,
                Endpoints = Endpoints.Count,
                Username = Configuration.Username
            };

            if (this is TunnelOutbound outboundTunnel)
            {
                prop.IsLoggedIn = outboundTunnel.IsLoggedIn;
                prop.ConnectionId = outboundTunnel.ConnectionId; //Outbound tunnels use a dedicated connection and do not have connectionIds.
            }
            else if (this is TunnelInbound inboundTunnel)
            {
                prop.IsLoggedIn = inboundTunnel.IsLoggedIn;
                prop.ConnectionId = inboundTunnel.ConnectionId;
            }

            return prop;
        }

        #endregion

        public TunnelConfiguration CloneConfiguration()
            => Configuration.CloneConfiguration();

        public virtual void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }
            Singletons.Logger.Verbose($"Starting tunnel '{Configuration.Name}'.");

            KeepRunning = true;
        }

        public virtual void Stop()
        {
            Singletons.Logger.Verbose($"Stopping tunnel '{Configuration.Name}'.");

            foreach (var endpoint in Endpoints)
            {
                Exceptions.Ignore(endpoint.Stop);
            }

            KeepRunning = false;

            Status = NtTunnelStatus.Stopped;

            Singletons.Logger.Verbose($"Stopped tunnel '{Configuration.Name}'.");
        }

        public void WriteEndpointEdgeData(Guid endpointId, Guid edgeId, long packetSequence, byte[] bytes)
            => Endpoints.Single(o => o.EndpointId == endpointId).WriteEndpointEdgeData(edgeId, packetSequence, bytes);

        #region Add/Delete Endpoints.

        public IEndpoint UpsertEndpoint(EndpointConfiguration configuration, string? username)
        {
            DeleteEndpoint(configuration.EndpointId, null);

            IEndpoint endpoint;

            if (configuration.Direction == NtDirection.Inbound)
            {
                endpoint = new EndpointInbound(ServiceEngine, this, configuration);
            }
            else if (configuration.Direction == NtDirection.Outbound)
            {
                endpoint = new EndpointOutbound(ServiceEngine, this, configuration);
            }
            else
            {
                throw new Exception("Endpoint direction is not well defined.");
            }

            lock (_endpointsLock)
            {
                _endpoints.Add(endpoint);
            }
            endpoint.Start();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointId, string? username)
        {
            IEndpoint? endpoint;
            lock (_endpointsLock)
            {
                endpoint = _endpoints.SingleOrDefault(o => o.EndpointId == endpointId);
                if (endpoint != null)
                {
                    _endpoints.Remove(endpoint);
                }
            }

            if (endpoint != null)
            {
                endpoint.Stop();

                //The user record owns the endpoint, so if we have been supplied one - delete it from the config.
                if (string.IsNullOrEmpty(username) == false)
                {
                    Singletons.ServiceEngine.Users.DeleteEndpoint(username, endpointId);
                }
            }
        }

        #endregion

        /// <summary>
        /// Disconnect the endpoint edge from the external server, browser, etc.
        /// </summary>
        public void DisconnectEndpointEdge(Guid endpointId, Guid edgeId)
            => Endpoints.SingleOrDefault(o => o.EndpointId == endpointId)?.Disconnect(edgeId);

        public override int GetHashCode()
        {
            int endpointHashes = int.MaxValue / 2;

            foreach (var endpoint in Endpoints)
            {
                endpointHashes = Utility.CombineHashes(endpointHashes, endpoint.GetHashCode());
            }

            return Utility.CombineHashes(
                [
                    Configuration.TunnelId.GetHashCode(),
                    Configuration.Name.GetHashCode(),
                    endpointHashes
                ]);
        }
    }
}
