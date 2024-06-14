using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.NullExtensions;
using NTDLS.Semaphore;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class BaseTunnelManager
    {
        public TunnelEngineCore Core { get; private set; }

        protected readonly PessimisticCriticalResource<List<Tunnel>> Collection = new();

        public void Start(Guid tunnelId) => Collection.Use((o) => o.Where(o => o.TunnelId == tunnelId).Single().Start());
        public void Stop(Guid tunnelId) => Collection.Use((o) => o.Where(o => o.TunnelId == tunnelId).Single().Stop());
        public void StartAll() => Collection.Use((o) => o.ForEach((o) => o.Start()));
        public void StopAll() => Collection.Use((o) => o.ForEach((o) => o.Stop()));

        public BaseTunnelManager(TunnelEngineCore core)
        {
            Core = core;
        }

        public void Add(NtTunnelConfiguration config)
        {
            Collection.Use((o) =>
            {
                var tunnel = new Tunnel(Core, config);
                o.Add(tunnel.EnsureNotNull());
            });
        }

        /*

        public void Delete(Guid tunnelId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                tunnel.Stop();
                o.Remove(tunnel);
            });
        }

        public void DispatchDeleteTunnel(Guid tunnelId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                tunnel.Notify(new oldNotificationDeleteTunnel(tunnelId));
            });
        }

        public void DeletePair(Guid tunnelId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                DispatchDeleteTunnel(tunnelId);
                tunnel.Stop();
                o.Remove(tunnel);
            });
        }

        */

        public List<NtTunnelStatistics> GetStatistics()
        {
            var result = new List<NtTunnelStatistics>();

            Collection.Use((o) =>
            {
                foreach (var tunnel in o)
                {
                    var tunnelStats = new NtTunnelStatistics()
                    {
                        Direction = Constants.NtDirection.Inbound, //TODO: Add some status for in/out bound tunnel.
                        Status = tunnel.Status,
                        TunnelId = tunnel.TunnelId,
                        BytesReceived = tunnel.BytesReceived,
                        BytesSent = tunnel.BytesSent,
                        CurrentConnections = tunnel.CurrentConnections,
                        TotalConnections = tunnel.TotalConnections,
                        ChangeHash = tunnel.GetHashCode()
                    };

                    foreach (var endpoint in tunnel.Endpoints)
                    {
                        var endpointStats = new NtEndpointStatistics()
                        {
                            Direction = endpoint is EndpointInbound ? Constants.NtDirection.Inbound : Constants.NtDirection.Outbound,
                            BytesReceived = endpoint.BytesReceived,
                            BytesSent = endpoint.BytesSent,
                            EndpointId = endpoint.EndpointId,
                            TunnelId = tunnel.TunnelId,
                            CurrentConnections = endpoint.CurrentConnections,
                            TotalConnections = endpoint.TotalConnections,
                            ChangeHash = endpoint.GetHashCode()
                        };
                        tunnelStats.EndpointStatistics.Add(endpointStats);
                    }

                    result.Add(tunnelStats);
                }
            });

            return result;
        }
    }
}
