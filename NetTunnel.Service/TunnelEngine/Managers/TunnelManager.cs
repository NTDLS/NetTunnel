using NetTunnel.Library;
using NetTunnel.Library.Types;
using NTDLS.NullExtensions;
using NTDLS.Persistence;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class TunnelManager : BaseTunnelManager
    {
        public TunnelManager(TunnelEngineCore core)
            : base(core)
        {
            LoadFromDisk();
        }

        public void Load(NtTunnelConfiguration config)
        {
            Collection.Use((o) =>
            {
                var tunnel = new Tunnel(Core, config);
                o.Add(tunnel.EnsureNotNull());
            });
        }

        public void Add(NtTunnelConfiguration config)
        {
            Collection.Use((o) =>
            {
                var tunnel = new Tunnel(Core, config);
                o.Add(tunnel.EnsureNotNull());
                SaveToDisk();
            });
        }

        public void UpsertEndpoint(NtEndpointConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == endpointConfiguration.TunnelId).Single();
                tunnel.UpsertEndpoint(endpointConfiguration);
                SaveToDisk();
            });
        }

        public void DeleteEndpoint(Guid tunnelId, Guid endpointId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                var endpoint = tunnel.Endpoints.Where(o => o.EndpointId == endpointId).Single();

                endpoint.Stop();

                tunnel.DeleteEndpoint(endpointId);
            });
        }

        public List<NtTunnelConfiguration> CloneConfigurations()
        {
            return Collection.Use((o) =>
            {
                List<NtTunnelConfiguration> clones = new();
                foreach (var tunnel in o)
                {
                    clones.Add(tunnel.CloneConfiguration());
                }
                return clones;
            });
        }

        public void SaveToDisk()
            => CommonApplicationData.SaveToDisk(Constants.FriendlyName, CloneConfigurations());

        private void LoadFromDisk()
        {
            Collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");
                CommonApplicationData.LoadFromDisk<List<NtTunnelConfiguration>>(Constants.FriendlyName)?.ForEach(o => Load(o));
            });
        }
    }
}
