using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.Semaphore;

namespace NetTunnel.Service.Engine.Managers
{
    public class TunnelOutboundManager
    {
        private readonly EngineCore _core;

        private readonly CriticalResource<List<TunnelOutbound>> _collection = new();

        public TunnelOutboundManager(EngineCore core)
        {
            _core = core;

            LoadFromDisk();
        }

        public void StartAll() => _collection.Use((o) => o.ForEach((o) => o.Start()));
        public void StopAll() => _collection.Use((o) => o.ForEach((o) => o.Stop()));

        public void DeleteInboundEndpoint(Guid tunnelPairId, Guid endpointPairId)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).First();
                tunnel.DeleteInboundEndpoint(endpointPairId);
            });
        }

        public void DeleteOutboundEndpoint(Guid tunnelPairId, Guid endpointPairId)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).First();
                tunnel.DeleteOutboundEndpoint(endpointPairId);
            });
        }

        public void AddInboundEndpoint(Guid tunnelPairId, NtEndpointInboundConfiguration configuration)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).First();
                tunnel.AddInboundEndpoint(configuration);
            });
        }

        public void AddOutboundEndpoint(Guid tunnelPairId, NtEndpointOutboundConfiguration configuration)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).First();
                tunnel.AddOutboundEndpoint(configuration);
            });
        }

        public void Add(NtTunnelOutboundConfiguration config)
        {
            _collection.Use((o) =>
            {
                var tunnel = new TunnelOutbound(_core, config);
                o.Add(tunnel);
                //tunnel.Start(); //We do not want to start the tunnels at construction, but rather by the engine Start() function.
            });
        }

        public void Delete(Guid tunnelPairId)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).First();
                tunnel.Stop();
                o.Remove(tunnel);
            });
        }

        public NtTunnelBasicInfo GetBasicInfo()
        {
            return _collection.Use((o) =>
            {
                return new NtTunnelBasicInfo
                {
                    //Name = o._con
                };
            });
        }

        public NtTunnelOutboundConfiguration CloneConfiguration(Guid tunnerPairId)
        {
            return _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnerPairId).Single();
                return tunnel.CloneConfiguration();
            });
        }

        public List<NtTunnelOutboundConfiguration> CloneConfigurations()
        {
            return _collection.Use((o) =>
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
            _collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");
                Persistence.LoadFromDisk<List<NtTunnelOutboundConfiguration>>()?.ForEach(o => Add(o));
            });
        }
    }
}
