using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.NullExtensions;
using NTDLS.Persistence;
using NTDLS.Semaphore;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class TunnelManager
    {
        private readonly ServiceEngine _serviceEngine;

        protected readonly PessimisticCriticalResource<List<ITunnel>> Collection = new();

        public TunnelManager(ServiceEngine serviceEngine)
        {
            _serviceEngine = serviceEngine;
            LoadFromDisk();
        }

        #region Start / Stop.

        public void Start(Guid tunnelId) => Collection.Use((o)
            => o.Single(o => o.Configuration.TunnelId == tunnelId).Start());
        public void Stop(Guid tunnelId) => Collection.Use((o)
            => o.Single(o => o.Configuration.TunnelId == tunnelId).Stop());

        public void StartAll()
            => Collection.Use((o) => o.ForEach((o) => o.Start()));

        public void StopAll()
            => Collection.Use((o) => o.ForEach((o) => o.Stop()));

        #endregion

        #region Create / Delete.

        /// <summary>
        /// The local service is adding a new outbound tunnel configuration to the local service.
        /// </summary>
        /// <param name="config"></param>
        public void UpsertTunnel(TunnelConfiguration config)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.Where(o => o.Configuration.TunnelId == config.TunnelId).SingleOrDefault();
                if (existingTunnel != null)
                {
                    existingTunnel.Stop();
                    o.Remove(existingTunnel);
                }

                var newTunnel = new TunnelOutbound(_serviceEngine, config);
                o.Add(newTunnel.EnsureNotNull());

                SaveToDisk();
            });
        }

        /// <summary>
        /// A remote service is registering its outbound tunnel configuration with the local service.
        /// </summary>
        /// <param name="config"></param>
        public void RegisterTunnel(Guid connectionId, TunnelConfiguration config)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.Where(o => o.Configuration.TunnelId == config.TunnelId).SingleOrDefault();
                if (existingTunnel != null)
                {
                    existingTunnel.Stop();
                    o.Remove(existingTunnel);
                }

                foreach (var endpoint in config.Endpoints)
                {
                    //Since we are receiving the endpoints from the other service, we need to flip the direction of their configuration.
                    endpoint.Direction = endpoint.Direction == NtDirection.Inbound ? NtDirection.Outbound : NtDirection.Inbound;
                }

                var newTunnel = new TunnelInbound(_serviceEngine, connectionId, config);
                o.Add(newTunnel.EnsureNotNull());

                newTunnel.Start();
            });
        }

        /// <summary>
        /// The local service is adding/editing an endpoint to a local outbound tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointConfiguration"></param>
        public void UpsertEndpoint(Guid tunnelId, EndpointConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.Configuration.TunnelId == tunnelId);
                tunnel.UpsertEndpoint(endpointConfiguration);

                SaveToDisk();
            });
        }

        /// <summary>
        /// The local service is deleting an endpoint from a local outbound tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        public void DeleteEndpoint(Guid tunnelId, Guid endpointId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.Configuration.TunnelId == tunnelId);
                tunnel.DeleteEndpoint(endpointId);

                SaveToDisk();
            });
        }

        #endregion

        /// <summary>
        /// A remote service is telling the local service that an inbound connection has been made to an
        /// inbound endpoint and is asking the local service to make the associated connection on the associated
        /// outbound endpoint.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        /// <param name="streamId"></param>
        /// <exception cref="Exception"></exception>
        public void EstablishOutboundEndpointConnection(Guid tunnelId, Guid endpointId, Guid streamId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.Configuration.TunnelId == tunnelId);

                var endpoint = tunnel.Endpoints.Single(o => o.EndpointId == endpointId) as EndpointOutbound
                    ?? throw new Exception("The endpoint could not be converted to outbound.");

                endpoint.EstablishOutboundEndpointConnection(streamId);
            });
        }

        public void SendEndpointData(Guid tunnelId, Guid endpointId, Guid StreamId, byte[] bytes)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.Configuration.TunnelId == tunnelId);
                var endpoint = tunnel.Endpoints.Single(o => o.EndpointId == endpointId);

                endpoint.SendEndpointData(StreamId, bytes);
            });
        }

        #region Disk Save/Load.

        /// <summary>
        /// Saves locally owned tunnels to disk.
        /// </summary>
        private void SaveToDisk()
        {
            var ownedConfiguration = Clone()
                .Where(o => o.ServiceId == Singletons.Configuration.ServiceId).ToList();

            CommonApplicationData.SaveToDisk(Constants.FriendlyName, ownedConfiguration);
        }

        private void LoadFromDisk()
        {
            Collection.Use((o) =>
            {
                o.Clear();

                CommonApplicationData.LoadFromDisk<List<TunnelConfiguration>>(FriendlyName)?
                    .ForEach(c =>
                    {
                        c.ServiceId = Singletons.Configuration.ServiceId; //Take ownership of tunnels if they are in the config file.
                        o.Add(new TunnelOutbound(_serviceEngine, c));
                    });
            });
        }

        #endregion

        public List<TunnelConfiguration> Clone()
        {
            return Collection.Use((o) =>
            {
                List<TunnelConfiguration> clones = new();
                foreach (var tunnel in o)
                {
                    clones.Add(tunnel.CloneConfiguration());
                }
                return clones;
            });
        }

        public List<TunnelStatistics> GetStatistics()
        {
            var result = new List<TunnelStatistics>();

            Collection.Use((o) =>
            {
                foreach (var tunnel in o)
                {
                    var tunnelStats = new TunnelStatistics()
                    {
                        Direction = tunnel.Configuration.ServiceId == Singletons.Configuration.ServiceId ? NtDirection.Outbound : NtDirection.Inbound,
                        Status = tunnel.Status,
                        TunnelId = tunnel.Configuration.TunnelId,
                        BytesReceived = tunnel.BytesReceived,
                        BytesSent = tunnel.BytesSent,
                        CurrentConnections = tunnel.CurrentConnections,
                        TotalConnections = tunnel.TotalConnections,
                        ChangeHash = tunnel.GetHashCode()
                    };

                    foreach (var endpoint in tunnel.Endpoints)
                    {
                        var endpointStats = new EndpointStatistics()
                        {
                            Direction = endpoint.Configuration.Direction,
                            BytesReceived = endpoint.BytesReceived,
                            BytesSent = endpoint.BytesSent,
                            EndpointId = endpoint.EndpointId,
                            TunnelId = tunnel.Configuration.TunnelId,
                            CurrentConnections = endpoint.CurrentConnections,
                            TotalConnections = endpoint.TotalConnections,
                            ChangeHash = endpoint.GetHashCode()
                        };
                        tunnelStats.EndpointStatistics.Add(endpointStats);
                    }

                    result.Add(tunnelStats);
                }
            });

            return result;
        }
    }
}
