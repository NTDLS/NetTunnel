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

        public void AddEndpoint(Guid tunnelPairId, NtEndpointInboundConfiguration configuration)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).First();
                tunnel.AddEndpoint(configuration);
            });
        }

        public void AddEndpoint(Guid tunnelPairId, NtEndpointOutboundConfiguration configuration)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).First();
                tunnel.AddEndpoint(configuration);
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
