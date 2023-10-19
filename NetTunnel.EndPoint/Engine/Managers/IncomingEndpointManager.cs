using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.Semaphore;

namespace NetTunnel.EndPoint.Engine.Managers
{
    public class IncomingEndpointManager
    {
        private readonly EngineCore _core;

        private readonly CriticalResource<List<IncomingEndpoint>> _collection = new();

        public IncomingEndpointManager(EngineCore core)
        {
            _core = core;

            LoadFromDisk();
        }

        public void StartAll() => _collection.Use((o) => o.ForEach((o) => o.Start()));

        public void StopAll() => _collection.Use((o) => o.ForEach((o) => o.Stop()));

        public void Add(NtIncomingEndpointConfig config)
        {
            _collection.Use((o) =>
            {
                var endpoint = new IncomingEndpoint(_core, config);
                o.Add(endpoint);
                //endpoint.Start(); //We do not want to start the endpoints at construction, but rather by the engine Start() function.
            });
        }

        public void Delete(Guid endpointId)
        {
            _collection.Use((o) =>
            {
                var endpoint = o.Where(o => o.Id == endpointId).First();
                endpoint.Stop();
                o.Remove(endpoint);
            });
        }

        public List<NtIncomingEndpointConfig> CloneConfigurations()
        {
            return _collection.Use((o) =>
            {
                List<NtIncomingEndpointConfig> clones = new();
                foreach (var endpoint in o)
                {
                    clones.Add(endpoint.CloneConfiguration());
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
                Persistence.LoadFromDisk<List<NtIncomingEndpointConfig>>()?.ForEach(o => Add(o));
            });
        }
    }
}
