﻿using NetTunnel.Library;
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

        public void Start(DirectionalKey tunnelKey)
            => Collection.Use((o) => o.Single(o => o.TunnelKey == tunnelKey).Start());

        public void Stop(DirectionalKey tunnelKey)
            => Collection.Use((o) => o.Single(o => o.TunnelKey == tunnelKey).Stop());

        public void StartAll()
            => Collection.Use((o) => o.ForEach((o) => o.Start()));

        public void StopAll()
            => Collection.Use((o) => o.ForEach((o) => o.Stop()));

        public TunnelPropertiesDisplay GetTunnelProperties(DirectionalKey tunnelKey)
            => Collection.Use((o) => o.Single(o => o.TunnelKey == tunnelKey).GetProperties());

        public EndpointPropertiesDisplay GetEndpointProperties(DirectionalKey tunnelKey, DirectionalKey endpointKey)
            => Collection.Use((o) => o.Single(o => o.TunnelKey == tunnelKey).GetEndpointProperties(endpointKey));

        public List<EndpointEdgeConnectionDisplay> GetEndpointEdgeConnections(DirectionalKey tunnelKey, DirectionalKey endpointKey)
            => Collection.Use((o) => o.Single(o => o.TunnelKey == tunnelKey).GetEndpointEdgeConnections(endpointKey));

        #endregion

        public void IncrementBytesSent(DirectionalKey tunnelKey, int bytes)
        {
            Collection.Use((o) =>
            {
                o.SingleOrDefault(o => o.TunnelKey == tunnelKey)?.IncrementBytesSent(bytes);
            });
        }

        public void IncrementBytesReceived(DirectionalKey tunnelKey, int bytes)
        {
            Collection.Use((o) =>
            {
                o.SingleOrDefault(o => o.TunnelKey == tunnelKey)?.IncrementBytesReceived(bytes);
            });
        }

        #region Create / Delete.

        /// <summary>
        /// The local service is adding a new outbound tunnel configuration to the local service.
        /// </summary>
        /// <param name="config"></param>
        public void UpsertTunnel(TunnelConfiguration config)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.SingleOrDefault(o => o.Configuration.TunnelId == config.TunnelId);
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

        public void DisconnectAndRemoveTunnel(DirectionalKey tunnelKey)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.SingleOrDefault(o => o.TunnelKey == tunnelKey);
                if (existingTunnel != null)
                {
                    existingTunnel.Stop();
                    o.Remove(existingTunnel);
                    SaveToDisk();
                }
            });
        }

        public void DeleteBothEndsOfTunnel(DirectionalKey tunnelKey)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.SingleOrDefault(o => o.TunnelKey == tunnelKey);
                if (existingTunnel != null)
                {
                    if (existingTunnel.IsLoggedIn)
                    {
                        //Let the other end of the tunnel know that we are deleting the tunnel.
                        existingTunnel.S2SPeerNotificationTunnelDeletion(tunnelKey.SwapDirection());
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
        public DirectionalKey RegisterTunnel(Guid connectionId, TunnelConfiguration config, List<EndpointConfiguration> endpoints)
        {
            return Collection.Use((o) =>
            {
                var existingTunnel = o.OfType<TunnelInbound>()
                    .SingleOrDefault(o => o.Configuration.TunnelId == config.TunnelId);
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

                var newTunnel = new TunnelInbound(_serviceEngine, connectionId, config);
                newTunnel.LoadEndpoints(endpoints);

                o.Add(newTunnel);

                newTunnel.Start();

                return newTunnel.TunnelKey;
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
                var existingTunnel = o.OfType<TunnelInbound>().SingleOrDefault(o => o.ConnectionId == connectionId);
                if (existingTunnel != null)
                {
                    existingTunnel.Stop();
                    o.Remove(existingTunnel);
                }
            });
        }

        public void UpdateLastPing(DirectionalKey tunnelKey, double pingTime)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.SingleOrDefault(o => o.TunnelKey == tunnelKey.SwapDirection());
                if (tunnel != null)
                {
                    tunnel.PingTime = pingTime;
                }
            });
        }

        /// <summary>
        /// The local service is adding/editing an endpoint to a local tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointConfiguration"></param>
        /// <param name="username">If the endpoint needs to saved to a user, then supply the username</param>
        public void UpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpointConfiguration, string? username)
        {
            var tunnel = Collection.Use((o) => o.Single(o => o.TunnelKey == tunnelKey));

            //Apply the endpoint here:
            tunnel.UpsertEndpoint(endpointConfiguration, username);

            if (tunnelKey.Direction == NtDirection.Inbound)
            {
                //Outbound tunnels own the configuration, so save it.
                if (string.IsNullOrEmpty(username) == false)
                {
                    Singletons.ServiceEngine.Users.UpsertEndpoint(username, endpointConfiguration);
                }
            }
        }

        /// <summary>
        /// The local service is adding/editing an endpoint to a local tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointConfiguration"></param>
        /// <param name="username">If the endpoint needs to saved to a user, then supply the username</param>
        public void UpsertEndpointAndDistribute(DirectionalKey tunnelKey, EndpointConfiguration endpointConfiguration, string? username)
        {
            var tunnel = Collection.Use((o) => o.Single(o => o.TunnelKey == tunnelKey));

            //Apply the endpoint here:
            UpsertEndpoint(tunnelKey, endpointConfiguration, username);

            //Apply the endpoint at the peer.
            tunnel.S2SPeerQueryUpsertEndpoint(tunnelKey.SwapDirection(), endpointConfiguration.SwapDirection());
        }

        /// <summary>
        /// The local service is deleting an endpoint from a local tunnel.
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        public void DeleteEndpointAndDistribute(DirectionalKey tunnelKey, Guid endpointId, string? username)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.TunnelKey == tunnelKey);
                if (tunnel.IsLoggedIn)
                {
                    //Let the other end of the tunnel know that we are deleting the endpoint.
                    tunnel.S2SPeerNotificationEndpointDeletion(tunnelKey.SwapDirection(), endpointId);
                }

                tunnel.DeleteEndpoint(endpointId, username);
            });
        }

        public void DeleteEndpoint(DirectionalKey tunnelKey, Guid endpointId, string? username)
        {
            Collection.Use((o) => o.SingleOrDefault(o => o.TunnelKey == tunnelKey)?.DeleteEndpoint(endpointId, username));
        }

        #endregion

        public void DisconnectEndpointEdge(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.SingleOrDefault(o => o.TunnelKey == tunnelKey);
                tunnel?.DisconnectEndpointEdge(endpointId, edgeId);
            });
        }

        /// <summary>
        /// A remote service is telling the local service that an inbound connection has been made to an
        /// inbound endpoint and is asking the local service to make the associated connection on the associated
        /// outbound endpoint.
        /// </summary>
        /// <param name="tunnelKey"></param>
        /// <param name="endpointId"></param>
        /// <param name="edgeId"></param>
        /// <exception cref="Exception"></exception>
        public void EstablishOutboundEndpointConnection(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.TunnelKey == tunnelKey);

                var endpoint = tunnel.Endpoints.Single(o => o.EndpointId == endpointId) as EndpointOutbound
                    ?? throw new Exception("The endpoint could not be converted to outbound.");

                endpoint.EstablishOutboundEndpointConnection(edgeId);
            });
        }

        public void WriteEndpointEdgeData(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, long packetSequence, byte[] bytes)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Single(o => o.TunnelKey == tunnelKey);
                var endpoint = tunnel.Endpoints.Single(o => o.EndpointId == endpointId);

                endpoint.WriteEndpointEdgeData(edgeId, packetSequence, bytes);
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
                        o.Add(new TunnelOutbound(_serviceEngine, c));
                    });
            });
        }

        #endregion

        public List<TunnelConfiguration> CloneOutbound()
        {
            return Collection.Use((o) =>
            {
                var clones = new List<TunnelConfiguration>();
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
                var results = new List<TunnelDisplay>();
                foreach (var tunnel in o)
                {
                    results.Add(new TunnelDisplay
                    {
                        IsLoggedIn = tunnel.IsLoggedIn,
                        TunnelId = tunnel.Configuration.TunnelId,
                        Address = tunnel.Configuration.Address,
                        Direction = tunnel is TunnelOutbound ? NtDirection.Outbound : NtDirection.Inbound,
                        Endpoints = tunnel.GetEndpointsForDisplay(),
                        ServicePort = tunnel.Configuration.ServicePort,
                        Name = tunnel.Configuration.Name,
                        ServiceId = tunnel.Configuration.ServiceId,
                    });
                }
                return results;
            });
        }

        public List<TunnelStatisticsDisplay> GetStatistics()
        {
            var result = new List<TunnelStatisticsDisplay>();

            Collection.Use((o) =>
            {
                foreach (var tunnel in o)
                {
                    var tunnelStats = new TunnelStatisticsDisplay()
                    {
                        TunnelKey = tunnel.TunnelKey,
                        Direction = tunnel is TunnelOutbound ? NtDirection.Outbound : NtDirection.Inbound,
                        Status = tunnel.Status,
                        TunnelId = tunnel.Configuration.TunnelId,
                        BytesReceived = tunnel.BytesReceived,
                        BytesSent = tunnel.BytesSent,
                        CurrentConnections = tunnel.CurrentConnections,
                        TotalConnections = tunnel.TotalConnections,
                        ChangeHash = tunnel.GetHashCode(),
                        PingTime = tunnel.PingTime
                    };

                    foreach (var endpoint in tunnel.Endpoints)
                    {
                        var endpointStats = new EndpointStatisticsDisplay()
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
