using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.Semaphore;

namespace NetTunnel.EndPoint.Engine.Managers
{
    public class IncommingEndpointManager
    {
        private readonly EngineCore _core;

        private readonly CriticalResource<List<NtIncommingEndpoint>> _collection = new();

        public IncommingEndpointManager(EngineCore core)
        {
            _core = core;

            LoadFromDisk();
        }

        public void Add(NtIncommingEndpoint endpoint)
        {
            _collection.Use((o) => o.Add(endpoint.Clone()));
        }

        public List<NtIncommingEndpoint> Clone()
        {
            return _collection.Use((o) =>
            {
                List<NtIncommingEndpoint> clones = new();
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

                Persistence.LoadFromDisk<List<NtIncommingEndpoint>>()?.ForEach(o => Add(o));
            });

        }
    }
}
