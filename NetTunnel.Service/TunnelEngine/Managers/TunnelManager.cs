using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.Helpers;
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

        public void Start(DirectionalKey tunnelKey) => Collection.Use((o)
            => o.Single(o => o.TunnelKey == tunnelKey).Start());

        public void Stop(DirectionalKey tunnelKey) => Collection.Use((o)
            => o.Single(o => o.TunnelKey == tunnelKey).Stop());

        public void StartAll()
            => Collection.Use((o)
                => o.ForEach((o) => o.Start()));

        public void StopAll()
            => Collection.Use((o)
                => o.ForEach((o) => o.Stop()));

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

                newTunnel.Start();
            });
        }

        public void DeleteTunnel(DirectionalKey tunnelKey)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.Where(o => o.TunnelKey == tunnelKey).SingleOrDefault();
                if (existingTunnel != null)
                {
                    if (existingTunnel.IsLoggedIn)
                    {
                        //Let the other end of the tunnel know that we are deleting the tunnel.
                        existingTunnel.PeerNotifyOfTunnelDeletion(tunnelKey.SwapDirection());
                    }

                    existingTunnel.Stop();
                    o.Remove(existingTunnel);
                    SaveToDisk();
                }
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
                var existingTunnel = o.OfType<TunnelInbound>()
                    .Where(o => o.Configuration.TunnelId == config.TunnelId).SingleOrDefault();
                if (existingTunnel != null)
                {
                    if (config.ServiceId == Singletons.Configuration.ServiceId)
                    {
                        //I'm not even sure if we can get here, but this is definitely an exception.
                        throw new Exception("This configuration is not supported.");
                    }
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
        /// A remote tunnel service has disconnected.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="config"></param>
        public void DeregisterTunnel(Guid connectionId)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.OfType<TunnelInbound>().Where(o => o.ConnectionId == connectionId).SingleOrDefault();
                if (existingTunnel != null)
                {
                    existingTunnel.Stop();
                    o.Remove(existingTunnel);
                }
            });
        }

        /// <summary>
        /// The local service is adding/editing an endpoint to a local tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointConfiguration"></param>
        public void UpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.TunnelKey == tunnelKey);
                tunnel.UpsertEndpoint(endpointConfiguration);

                SaveToDisk();
            });
        }

        /// <summary>
        /// The local service is deleting an endpoint from a local tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        public void DeleteEndpoint(DirectionalKey tunnelKey, Guid endpointId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.TunnelKey == tunnelKey);

                if (tunnel.IsLoggedIn)
                {
                    //Let the other end of the tunnel know that we are deleting the endpoint.
                    tunnel.PeerNotifyOfEndpointDeletion(tunnelKey.SwapDirection(), endpointId);
                }

                tunnel.DeleteEndpoint(endpointId);

                SaveToDisk();
            });
        }

        #endregion

        /// <summary>
        /// Returns true if the local service owns the tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <returns></returns>
        public bool IsOwned(Guid tunnelId)
        {
            return Collection.Use((o) =>
            {
                return o.Single(o => o.Configuration.TunnelId == tunnelId).Configuration.ServiceId == Singletons.Configuration.ServiceId;
            });
        }

        /// <summary>
        /// A remote service is telling the local service that an inbound connection has been made to an
        /// inbound endpoint and is asking the local service to make the associated connection on the associated
        /// outbound endpoint.
        /// </summary>
        /// <param name="tunnelKey"></param>
        /// <param name="endpointId"></param>
        /// <param name="streamId"></param>
        /// <exception cref="Exception"></exception>
        public void EstablishOutboundEndpointConnection(DirectionalKey tunnelKey, Guid endpointId, Guid streamId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.TunnelKey == tunnelKey);

                var endpoint = tunnel.Endpoints.Single(o => o.EndpointId == endpointId) as EndpointOutbound
                    ?? throw new Exception("The endpoint could not be converted to outbound.");

                endpoint.EstablishOutboundEndpointConnection(streamId);
            });
        }

        public void WriteEndpointEdgeData(DirectionalKey tunnelKey, Guid endpointId, Guid StreamId, byte[] bytes)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.TunnelKey == tunnelKey);
                var endpoint = tunnel.Endpoints.Single(o => o.EndpointId == endpointId);

                endpoint.WriteEndpointEdgeData(StreamId, bytes);
            });
        }

        #region Disk Save/Load.

        /// <summary>
        /// Saves locally owned tunnels to disk.
        /// </summary>
        private void SaveToDisk()
        {
            var ownedConfiguration = CloneOutbound()
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
                        c.TunnelId = Guid.NewGuid(); //Tunnels get a new ID every time they are loaded. This makes it easy to copy configs to other machines.
                        c.Endpoints.ForEach(e => e.EndpointId = Guid.NewGuid()); //Endpoints get a new ID every time they are loaded. This makes it easy to copy configs to other machines.
                        o.Add(new TunnelOutbound(_serviceEngine, c));
                    });
            });
        }

        #endregion

        public List<TunnelConfiguration> CloneOutbound()
        {
            return Collection.Use((o) =>
            {
                List<TunnelConfiguration> clones = new();
                foreach (var tunnel in o.OfType<TunnelOutbound>())
                {
                    clones.Add(tunnel.CloneConfiguration());
                }
                return clones;
            });
        }

        public List<TunnelDisplay> GetForDisplay()
        {
            return Collection.Use((o) =>
            {
                List<TunnelDisplay> results = new();
                foreach (var tunnel in o)
                {
                    results.Add(new TunnelDisplay
                    {
                        IsLoggedIn = tunnel.IsLoggedIn,
                        TunnelId = tunnel.Configuration.TunnelId,
                        Address = tunnel.Configuration.Address,
                        Direction = tunnel is TunnelOutbound ? NtDirection.Outbound : NtDirection.Inbound,
                        Endpoints = tunnel.Configuration.GetEndpointsForDisplay(),
                        ManagementPort = tunnel.Configuration.ManagementPort,
                        Name = tunnel.Configuration.Name,
                        ServiceId = tunnel.Configuration.ServiceId,
                    });
                }
                return results;
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
                        TunnelKey = tunnel.TunnelKey,
                        Direction = tunnel is TunnelOutbound ? NtDirection.Outbound : NtDirection.Inbound,
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
                            EndpointKey = endpoint.EndpointKey,
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
