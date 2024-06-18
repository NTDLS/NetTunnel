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
        private readonly ServiceEngine _Core;

        protected readonly PessimisticCriticalResource<List<Tunnel>> Collection = new();

        public TunnelManager(ServiceEngine core)
        {
            _Core = core;
            LoadFromDisk();
        }

        #region Start / Stop.

        public void Start(Guid tunnelId) => Collection.Use((o)
            => o.Where(o => o.Configuration.TunnelId == tunnelId).Single().Start());
        public void Stop(Guid tunnelId) => Collection.Use((o)
            => o.Where(o => o.Configuration.TunnelId == tunnelId).Single().Stop());

        public void StartAll()
            => Collection.Use((o) => o.ForEach((o) => o.Start()));

        public void StopAll()
            => Collection.Use((o) => o.ForEach((o) => o.Stop()));

        #endregion

        #region Create / Delete.

        public void UpsertTunnel(NtTunnelConfiguration config)
        {
            Collection.Use((o) =>
            {
                var existingTunnel = o.Where(o => o.Configuration.TunnelId == config.TunnelId).SingleOrDefault();
                if (existingTunnel != null)
                {
                    existingTunnel.Stop();
                    o.Remove(existingTunnel);
                }

                var newTunnel = new Tunnel(_Core, config);
                o.Add(newTunnel.EnsureNotNull());

                SaveToDisk();
            });
        }

        public void UpsertEndpoint(Guid tunnelId, NtEndpointConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.Configuration.TunnelId == tunnelId).Single();
                tunnel.UpsertEndpoint(endpointConfiguration);

                SaveToDisk();
            });
        }

        public void DeleteEndpoint(Guid tunnelId, Guid endpointId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.Configuration.TunnelId == tunnelId).Single();
                tunnel.DeleteEndpoint(endpointId);

                SaveToDisk();
            });
        }

        #endregion

        public void EstablishOutboundEndpointConnection(Guid tunnelId, Guid endpointId, Guid streamId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.Configuration.TunnelId == tunnelId).Single();

                var endpoint = tunnel.Endpoints.Where(o => o.EndpointId == endpointId).Single() as EndpointOutbound
                    ?? throw new Exception("The endpoint could not be converted to outbound.");

                endpoint.EstablishOutboundEndpointConnection(streamId);
            });
        }

        #region Disk Save/Load.

        /// <summary>
        /// Saves locally owned tunnels to disk.
        /// </summary>
        private void SaveToDisk()
        {
            var clonedConfig = Clone()
                .Where(o => o.ServiceId == Singletons.Configuration.ServiceId).ToList();

            CommonApplicationData.SaveToDisk(Constants.FriendlyName, Clone());
        }

        private void LoadFromDisk()
        {
            Collection.Use((o) =>
            {
                o.Clear();

                CommonApplicationData.LoadFromDisk<List<NtTunnelConfiguration>>(Constants.FriendlyName)?
                    .Where(t => t.ServiceId == Singletons.Configuration.ServiceId).ToList()
                    .ForEach(c => o.Add(new Tunnel(_Core, c)));
            });
        }

        #endregion

        public List<NtTunnelConfiguration> Clone()
        {
            return Collection.Use((o) =>
            {
                List<NtTunnelConfiguration> clones = new();
                foreach (var tunnel in o)
                {
                    clones.Add(tunnel.CloneConfiguration());
                }
                return clones;
            });
        }

        public List<NtTunnelStatistics> GetStatistics()
        {
            var result = new List<NtTunnelStatistics>();

            Collection.Use((o) =>
            {
                foreach (var tunnel in o)
                {
                    var tunnelStats = new NtTunnelStatistics()
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
                        var endpointStats = new NtEndpointStatistics()
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
