using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.MessageFraming.FramePayloads.Notifications;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.Semaphore;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class BaseTunnelManager<T, C> where T : ITunnel where C : INtTunnelConfiguration
    {
        public TunnelEngineCore Core { get; private set; }

        protected readonly CriticalResource<List<T>> Collection = new();

        public void Start(Guid tunnelPairId) => Collection.Use((o) => o.Where(o => o.PairId == tunnelPairId).Single().Start());
        public void Stop(Guid tunnelPairId) => Collection.Use((o) => o.Where(o => o.PairId == tunnelPairId).Single().Stop());
        public void StartAll() => Collection.Use((o) => o.ForEach((o) => o.Start()));
        public void StopAll() => Collection.Use((o) => o.ForEach((o) => o.Stop()));

        public BaseTunnelManager(TunnelEngineCore core)
        {
            Core = core;
        }

        public void Add(C config)
        {
            Collection.Use((o) =>
            {
                var tunnel = (T?)Activator.CreateInstance(typeof(T), Core, config);
                Utility.EnsureNotNull(tunnel);
                o.Add(tunnel);
            });
        }

        public void Delete(Guid tunnelPairId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                tunnel.Stop();
                o.Remove(tunnel);
            });
        }

        public void DispatchDeleteTunnel(Guid tunnelPairId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                tunnel.SendStreamFrameNotification(new NtFramePayloadDeleteTunnel(tunnelPairId));
            });
        }

        public void DeletePair(Guid tunnelPairId)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                DispatchDeleteTunnel(tunnelPairId);
                tunnel.Stop();
                o.Remove(tunnel);
            });
        }

        public List<NtTunnelStatistics> GetStatistics()
        {
            var result = new List<NtTunnelStatistics>();

            Collection.Use((o) =>
            {
                foreach (var tunnel in o)
                {
                    var tunnelStats = new NtTunnelStatistics()
                    {
                        Direction = typeof(T) == typeof(TunnelInbound) ? Constants.NtDirection.Inbound : Constants.NtDirection.Outbound,
                        Status = tunnel.Status,
                        TunnelPairId = tunnel.PairId,
                        BytesReceived = tunnel.BytesReceived,
                        BytesSent = tunnel.BytesSent,
                        CurrentConnections = tunnel.CurrentConnections,
                        TotalConnections = tunnel.TotalConnections
                    };

                    foreach (var endpoint in tunnel.Endpoints)
                    {
                        var endpointStats = new NtEndpointStatistics()
                        {
                            Direction = endpoint is EndpointInbound ? Constants.NtDirection.Inbound : Constants.NtDirection.Outbound,
                            BytesReceived = endpoint.BytesReceived,
                            BytesSent = endpoint.BytesSent,
                            EndpointPairId = endpoint.PairId,
                            TunnelPairId = tunnel.PairId,
                            CurrentConnections = endpoint.CurrentConnections,
                            TotalConnections = endpoint.TotalConnections
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
