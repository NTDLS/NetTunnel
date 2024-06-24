using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Query;
using NetTunnel.Service.TunnelEngine.Endpoints;
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

        public int ChangeHash
            => Configuration.TunnelId.GetHashCode()
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
        public List<IEndpoint> Endpoints { get; private set; } = new();
        bool ITunnel.IsLoggedIn => throw new NotImplementedException("This function should be overridden.");


        #endregion

        public TunnelBase(ServiceEngine serviceEngine, TunnelConfiguration configuration)
        {
            ServiceEngine = serviceEngine;
            Configuration = configuration.CloneConfiguration();

            Configuration.Endpoints.Where(o => o.Direction == NtDirection.Inbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointInbound(ServiceEngine, this, o)));

            Configuration.Endpoints.Where(o => o.Direction == NtDirection.Outbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointOutbound(ServiceEngine, this, o)));
        }

        #region Interface: ITunnel.

        void ITunnel.IncrementBytesSent(int bytes)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.IncrementBytesReceived(int bytes)
           => throw new NotImplementedException("This function should be overridden.");

        QueryUpsertEndpointReply ITunnel.PeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpointId)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.PeerNotifyOfEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.PeerNotifyOfEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.PeerNotifyOfTunnelDeletion(DirectionalKey tunnelKey)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.PeerNotifyOfEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
           => throw new NotImplementedException("This function should be overridden.");

        void ITunnel.PeerNotifyOfEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
           => throw new NotImplementedException("This function should be overridden.");

        public TunnelStatisticsProperties GetProperties()
        {
            var serviceConnectionState = Singletons.ServiceEngine
                .ServiceConnectionStates.SingleOrDefault(o => o.Value.TunnelKey?.Id == TunnelKey.Id).Value;

            var prop = new TunnelStatisticsProperties()
            {
                KeyHash = serviceConnectionState.KeyHash,
                KeyLength = serviceConnectionState.KeyLength,
                BytesReceived = BytesReceived,
                BytesSent = BytesSent,
                CurrentConnections = CurrentConnections,
                Direction = Direction,
                PingTime = PingTime,
                Status = Status,
                TotalConnections = TotalConnections,
                TunnelKey = TunnelKey,
                PeerIpAddress = serviceConnectionState.ClientIpAddress,
                ClientIpAddress = serviceConnectionState.ClientIpAddress,
                IsAuthenticated = serviceConnectionState.IsAuthenticated,
                KeepRunning = KeepRunning,
                LoginTime = serviceConnectionState.LoginTime,
                SecureKeyExchangeIsComplete = serviceConnectionState.SecureKeyExchangeIsComplete,
                LoggedInUserName = serviceConnectionState.UserName,
                ServiceId = Configuration.ServiceId,
                Name = Configuration.Name,
                Endpoints = Configuration.Endpoints.Count
            };

            if (this is TunnelOutbound outboundTunnel)
            {
                prop.IsLoggedIn = outboundTunnel.IsLoggedIn;
                prop.ConnectionId = Guid.Empty; //Outbound tunnels use a dedicated connection and do not have connectionIds.

                prop.OutboundAddress = Configuration.Address;
                prop.OutboundPort = Configuration.ServicePort;
                prop.OutboundUsername = Configuration.Username;
            }
            else if (this is TunnelInbound inboundTunnel)
            {
                prop.IsLoggedIn = inboundTunnel.IsLoggedIn;
                prop.ConnectionId = inboundTunnel.ConnectionId;

                prop.InboundAddress = Configuration.Address;
                prop.InboundPort = Configuration.ServicePort;
                prop.InboundUsername = Configuration.Username;
            }

            return prop;
        }

        #endregion

        public IEndpoint? GetEndpointById(Guid pairId)
            => Endpoints.SingleOrDefault(o => o.EndpointId == pairId);

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

            Singletons.Logger.Verbose($"Starting endpoints for tunnel '{Configuration.Name}'.");

            Endpoints.ForEach(x => x.Start());
        }

        public virtual void Stop()
        {
            Singletons.Logger.Verbose($"Stopping tunnel '{Configuration.Name}'.");

            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;

            Status = NtTunnelStatus.Stopped;

            Singletons.Logger.Verbose($"Stopped tunnel '{Configuration.Name}'.");
        }

        public void WriteEndpointEdgeData(Guid endpointId, Guid edgeId, byte[] bytes)
            => Endpoints.Single(o => o.EndpointId == endpointId).WriteEndpointEdgeData(edgeId, bytes);

        #region Add/Delete Endpoints.

        public IEndpoint UpsertEndpoint(EndpointConfiguration configuration)
        {
            var existingEndpoint = GetEndpointById(configuration.EndpointId);
            if (existingEndpoint != null)
            {
                DeleteEndpoint(existingEndpoint.EndpointId);
            }

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

            Configuration.Endpoints.Add(configuration);
            Endpoints.Add(endpoint);
            endpoint.Start();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointId)
        {
            var endpoint = GetEndpointById(endpointId);
            if (endpoint != null)
            {
                endpoint.Stop();
                Configuration.Endpoints.RemoveAll(o => o.EndpointId == endpointId);
                Endpoints.Remove(endpoint);
            }
        }

        #endregion

        /// <summary>
        /// Disconnect the endpoint edge from the external server, browser, etc.
        /// </summary>
        public void DisconnectEndpointEdge(Guid endpointId, Guid edgeId)
            => GetEndpointById(endpointId)?.Disconnect(edgeId);

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
