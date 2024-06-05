using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.Persistence;

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
        /// Tell the remote tunnel service to add the inbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelPairId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T?> DispatchAddEndpointInboundToAssociatedTunnelService<T>(Guid tunnelPairId, NtEndpointInboundConfiguration endpoint)
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                return tunnel.SendStreamFramePayloadQuery<T>(new NtFramePayloadAddEndpointInbound(endpoint));
            });
        }

        /// <summary>
        /// Tell the remote tunnel service to add the outbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelPairId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T?> DispatchAddEndpointOutboundToAssociatedTunnelService<T>(Guid tunnelPairId, NtEndpointOutboundConfiguration endpoint)
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                return tunnel.SendStreamFramePayloadQuery<T>(new NtFramePayloadAddEndpointOutbound(endpoint));
            });
        }

        public void AddEndpointInbound(Guid tunnelPairId, NtEndpointInboundConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                var endpoint = tunnel.AddInboundEndpoint(endpointConfiguration);
                endpoint.Start();
            });
        }

        public void AddEndpointOutbound(Guid tunnelPairId, NtEndpointOutboundConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                var endpoint = tunnel.AddOutboundEndpoint(endpointConfiguration);
                endpoint.Start();
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
