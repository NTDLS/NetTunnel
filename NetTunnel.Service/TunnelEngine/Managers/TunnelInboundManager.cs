﻿using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.Semaphore;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    public class TunnelInboundManager
    {
        private readonly TunnelEngineCore _core;

        private readonly CriticalResource<List<TunnelInbound>> _collection = new();

        public TunnelInboundManager(TunnelEngineCore core)
        {
            _core = core;

            LoadFromDisk();
        }

        public void Start(Guid tunnelPairId) => _collection.Use((o) => o.Where(o => o.PairId == tunnelPairId).Single().Start());
        public void Stop(Guid tunnelPairId) => _collection.Use((o) => o.Where(o => o.PairId == tunnelPairId).Single().Stop());
        public void StartAll() => _collection.Use((o) => o.ForEach((o) => o.Start()));
        public void StopAll() => _collection.Use((o) => o.ForEach((o) => o.Stop()));

        public void Add(NtTunnelInboundConfiguration config)
        {
            _collection.Use((o) =>
            {
                var tunnel = new TunnelInbound(_core, config);
                o.Add(tunnel);
                //tunnel.Start(); //We do not want to start the tunnels at construction, but rather by the engine Start() function.
            });
        }

        public void Delete(Guid tunnelPairId)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                tunnel.Stop();
                o.Remove(tunnel);
            });
        }

        /// <summary>
        /// Tell the remote tunnel service to add the inbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelPairId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T?> DispatchAddEndpointInboundToAssociatedTunnelService<T>(Guid tunnelPairId, NtEndpointInboundConfiguration endpoint)
        {
            return await _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                return tunnel.SendStreamFramePayloadQuery<T>(new NtFramePayloadAddEndpointInbound(endpoint));
            });
        }

        /// <summary>
        /// Tell the remote tunnel service to add the outbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelPairId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T?> DispatchAddEndpointOutboundToAssociatedTunnelService<T>(Guid tunnelPairId, NtEndpointOutboundConfiguration endpoint)
        {
            return await _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                return tunnel.SendStreamFramePayloadQuery<T>(new NtFramePayloadAddEndpointOutbound(endpoint));
            });
        }

        public void AddEndpointInbound(Guid tunnelPairId, NtEndpointInboundConfiguration endpointConfiguration)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                var endpoint = tunnel.AddInboundEndpoint(endpointConfiguration);
                endpoint.Start();
            });
        }

        public void AddEndpointOutbound(Guid tunnelPairId, NtEndpointOutboundConfiguration endpointConfiguration)
        {
            _collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.PairId == tunnelPairId).Single();
                var endpoint = tunnel.AddOutboundEndpoint(endpointConfiguration);
                endpoint.Start();
            });
        }

        public List<NtTunnelStatistics> GetStatistics()
        {
            var result = new List<NtTunnelStatistics>();

            _collection.Use((o) =>
            {
                foreach (var tunnel in o)
                {
                    var tunnelStats = new NtTunnelStatistics()
                    {
                        Direction = Constants.NtDirection.Inbound,
                        TunnelPairId = tunnel.PairId,
                        BytesReceived = 0, //TODO: Fill me in.
                        BytesSent = 0, //TODO: Fill me in.
                        CurrentConnections = 0, //TODO: Fill me in.
                        TotalConnections = 0 //TODO: Fill me in.
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
                            CurrentConnections = 0, //TODO: Fill me in.
                            TotalConnections = 0 //TODO: Fill me in.
                        };
                        tunnelStats.EndpointStatistics.Add(endpointStats);
                    }

                    result.Add(tunnelStats);
                }
            });

            return result;
        }

        public List<NtTunnelInboundConfiguration> CloneConfigurations()
        {
            return _collection.Use((o) =>
            {
                List<NtTunnelInboundConfiguration> clones = new();
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
                Persistence.LoadFromDisk<List<NtTunnelInboundConfiguration>>()?.ForEach(o => Add(o));
            });
        }
    }
}
