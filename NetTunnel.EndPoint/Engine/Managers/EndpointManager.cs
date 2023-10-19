using NetTunnel.Library.Types;
using NTDLS.Semaphore;

namespace NetTunnel.EndPoint.Engine.Managers
{
    public class EndpointManager
    {
        private readonly EngineCore _core;

        public CriticalResource<List<NtEndpoint>> Collection { get; set; } = new();

        public EndpointManager(EngineCore core)
        {
            _core = core;
        }

        public List<NtEndpoint> Clone()
        {
            return Collection.Use((o) =>
            {
                List<NtEndpoint> clones = new();
                foreach (var endpoint in o)
                {
                    clones.Add(endpoint.Clone());
                }
                return clones;
            });
        }
    }
}
