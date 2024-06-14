using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages.Query;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.NullExtensions;
using NTDLS.Persistence;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class TunnelInboundManager : BaseTunnelManager<TunnelInbound, NtTunnelInboundConfiguration>
    {
        public TunnelInboundManager(TunnelEngineCore core)
            : base(core)
        {
            LoadFromDisk();
        }

        /// <summary>
        /// Tell the remote tunnel service to delete the specified endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        /// <returns></returns>
        public async Task<T> DispatchDeleteEndpointToAssociatedTunnelService<T>(Guid tunnelId, Guid endpointId) where T : class, IRmQueryReply
        {
            return (await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                return tunnel.Query(new oldQueryDeleteEndpoint(endpointId));
            }) as T).EnsureNotNull();
        }

        /// <summary>
        /// Tell the remote tunnel service to add the inbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T> DispatchUpsertEndpointInboundToAssociatedTunnelService<T>(Guid tunnelId, NtEndpointInboundConfiguration endpoint) where T : class, IRmQueryReply
        {
            return (await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                return tunnel.Query(new oldQueryUpsertEndpointInbound(endpoint));
            }) as T).EnsureNotNull();
        }

        /// <summary>
        /// Tell the remote tunnel service to add the outbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T> DispatchUpsertEndpointOutboundToAssociatedTunnelService<T>(Guid tunnelId, NtEndpointOutboundConfiguration endpoint) where T : class, IRmQueryReply
        {
            return (await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                return tunnel.Query(new oldQueryUpsertEndpointOutbound(endpoint));
            }) as T).EnsureNotNull();
        }

        public void UpsertEndpointInbound(Guid tunnelId, NtEndpointInboundConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                var endpoint = tunnel.UpsertInboundEndpoint(endpointConfiguration);
                endpoint.Start();
            });
        }

        public void UpsertEndpointOutbound(Guid tunnelId, NtEndpointOutboundConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                var endpoint = tunnel.UpsertOutboundEndpoint(endpointConfiguration);
                endpoint.Start();
            });
        }

        public void DeleteEndpoint(Guid tunnelId, Guid endpointId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                var endpoint = tunnel.Endpoints.Where(o => o.EndpointId == endpointId).Single();

                endpoint.Stop();

                tunnel.DeleteEndpoint(endpointId);
            });
        }

        public List<NtTunnelInboundConfiguration> CloneConfigurations()
        {
            return Collection.Use((o) =>
            {
                List<NtTunnelInboundConfiguration> clones = new();
                foreach (var tunnel in o)
                {
                    clones.Add(tunnel.CloneConfiguration());
                }
                return clones;
            });
        }

        public void SaveToDisk() => CommonApplicationData.SaveToDisk(Constants.FriendlyName, CloneConfigurations());

        private void LoadFromDisk()
        {
            Collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");
                CommonApplicationData.LoadFromDisk<List<NtTunnelInboundConfiguration>>(Constants.FriendlyName)?.ForEach(o => Add(o));
            });
        }
    }
}
