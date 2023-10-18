using NetTunnel.Library.Types;
using NTDLS.Semaphore;

namespace NetTunnel.Engine.Managers
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
            List<NtEndpoint> clones = new();

            Collection.Use((o) =>
            {
                foreach (var endpoint in o)
                {
                    clones.Add(endpoint.Clone());
                }
            });

            return clones;
        }

    }
}
