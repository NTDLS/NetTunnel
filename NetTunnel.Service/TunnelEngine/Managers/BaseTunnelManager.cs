using NetTunnel.Library;
using NetTunnel.Library.Types;
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

        public async Task<R?> DispatchDeleteTunnel<R>(Guid tunnelPairId)
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                return tunnel.SendStreamFramePayloadQuery<R>(new NtFramePayloadDeleteTunnel(tunnelPairId));
            });
        }

        public async Task<bool> DeletePair(Guid tunnelPairId)
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();

                return DispatchDeleteTunnel<bool>(tunnelPairId).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully && t.Result == true)
                    {
                        tunnel.Stop();
                        o.Remove(tunnel);
                        return true;
                    }
                    return false;
                });
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
