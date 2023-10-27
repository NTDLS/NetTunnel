using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.TunnelEngine.Tunnels;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class TunnelOutboundManager : BaseTunnelManager<TunnelOutbound, NtTunnelOutboundConfiguration>
    {
        public TunnelOutboundManager(TunnelEngineCore core)
            : base(core)
        {
            LoadFromDisk();
        }

        public async Task<T?> DispatchAddEndpointInbound<T>(Guid tunnelPairId, NtEndpointInboundConfiguration endpoint)
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                return tunnel.SendStreamFramePayloadQuery<T>(new NtFramePayloadAddEndpointInbound(endpoint));
            });
        }

        public async Task<T?> DispatchAddEndpointOutbound<T>(Guid tunnelPairId, NtEndpointOutboundConfiguration endpoint)
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                return tunnel.SendStreamFramePayloadQuery<T>(new NtFramePayloadAddEndpointOutbound(endpoint));
            });
        }

        public void AddEndpointInbound(Guid tunnelPairId, NtEndpointInboundConfiguration endpoint)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                tunnel.AddInboundEndpoint(endpoint);
            });
        }

        public void AddEndpointOutbound(Guid tunnelPairId, NtEndpointOutboundConfiguration endpoint)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                tunnel.AddOutboundEndpoint(endpoint);
                //tunnel.Start();
            });
        }

        public List<NtTunnelOutboundConfiguration> CloneConfigurations()
        {
            return Collection.Use((o) =>
            {
                List<NtTunnelOutboundConfiguration> clones = new();
                foreach (var tunnel in o)
                {
                    clones.Add(tunnel.CloneConfiguration());
                }
                return clones;
            });
        }

        public void SaveToDisk() => Persistence.SaveToDisk(CloneConfigurations());

        private void LoadFromDisk()
        {
            Collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");
                Persistence.LoadFromDisk<List<NtTunnelOutboundConfiguration>>()?.ForEach(o => Add(o));
            });
        }
    }
}
