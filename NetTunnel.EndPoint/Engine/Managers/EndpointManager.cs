using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.Semaphore;

namespace NetTunnel.EndPoint.Engine.Managers
{
    public class EndpointManager
    {
        private readonly EngineCore _core;

        private readonly CriticalResource<List<NtEndpoint>> _collection = new();

        public EndpointManager(EngineCore core)
        {
            _core = core;

            LoadFromDisk();
        }

        public void Add(NtEndpoint endpoint)
        {
            _collection.Use((o) => o.Add(endpoint.Clone()));
        }

        public List<NtEndpoint> Clone()
        {
            return _collection.Use((o) =>
            {
                List<NtEndpoint> clones = new();
                foreach (var endpoint in o)
                {
                    clones.Add(endpoint);
                }
                return clones;
            });
        }

        public void SaveToDisk() => Persistence.SaveToDisk(Clone());

        private void LoadFromDisk()
        {
            _collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");

                Persistence.LoadFromDisk<List<NtEndpoint>>()?.ForEach(o => Add(o));
            });

        }
    }
}
