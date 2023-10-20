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

        public void Add(NtTunnelOutboundConfig config)
        {
            _collection.Use((o) =>
            {
                var tunnel = new TunnelOutbound(_core, config);
                o.Add(tunnel);
                //tunnel.Start(); //We do not want to start the tunnels at construction, but rather by the engine Start() function.
            });
        }

        public void Delete(Guid tunnelId)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.Id == tunnelId).First();
                tunnel.Stop();
                o.Remove(tunnel);
            });
        }

        public List<NtTunnelOutboundConfig> CloneConfigurations()
        {
            return _collection.Use((o) =>
            {
                List<NtTunnelOutboundConfig> clones = new();
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
                Persistence.LoadFromDisk<List<NtTunnelOutboundConfig>>()?.ForEach(o => Add(o));
            });
        }
    }
}
