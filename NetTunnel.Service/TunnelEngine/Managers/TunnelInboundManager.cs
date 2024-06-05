﻿using Microsoft.AspNetCore.Hosting.Server;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.FramePayloads.Queries;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.Persistence;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class TunnelInboundManager : BaseTunnelManager<TunnelInbound, NtTunnelInboundConfiguration>
    {
        public TunnelInboundManager(TunnelEngineCore core)
            : base(core)
        {
            LoadFromDisk();
        }

        /// <summary>
        /// Tell the remote tunnel service to delete the specified endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelId"></param>
        /// <param name="endpointId"></param>
        /// <returns></returns>
        public async Task<T> DispatchDeleteEndpointToAssociatedTunnelService<T>(Guid tunnelId, Guid endpointId) where T : IRmQueryReply
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                return tunnel.Query<T>(new NtFramePayloadDeleteEndpoint(endpointId));
            });
        }

        /// <summary>
        /// Tell the remote tunnel service to add the inbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T> DispatchAddEndpointInboundToAssociatedTunnelService<T>(Guid tunnelId, NtEndpointInboundConfiguration endpoint) where T : IRmQueryReply
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();

                SameSignatureQuery(new NtFramePayloadAddEndpointInbound(endpoint));

                return tunnel.Query<T>(new NtFramePayloadAddEndpointInbound(endpoint));
            });
        }

        public Task<T> SameSignatureQuery<T>(IRmQuery<T> query) where T : IRmQueryReply
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tell the remote tunnel service to add the outbound endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tunnelId"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<T> DispatchAddEndpointOutboundToAssociatedTunnelService<T>(Guid tunnelId, NtEndpointOutboundConfiguration endpoint) where T : IRmQueryReply
        {
            return await Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                return tunnel.Query<T>(new NtFramePayloadAddEndpointOutbound(endpoint));
            });
        }

        public void AddEndpointInbound(Guid tunnelId, NtEndpointInboundConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                var endpoint = tunnel.AddInboundEndpoint(endpointConfiguration);
                endpoint.Start();
            });
        }

        public void AddEndpointOutbound(Guid tunnelId, NtEndpointOutboundConfiguration endpointConfiguration)
        {
            Collection.Use((o) =>
            {
                var tunnel = o.Where(o => o.TunnelId == tunnelId).Single();
                var endpoint = tunnel.AddOutboundEndpoint(endpointConfiguration);
                endpoint.Start();
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

                endpoint.Start();
            });
        }

        public List<NtTunnelInboundConfiguration> CloneConfigurations()
        {
            return Collection.Use((o) =>
            {
                List<NtTunnelInboundConfiguration> clones = new();
                foreach (var tunnel in o)
                {
                    clones.Add(tunnel.CloneConfiguration());
                }
                return clones;
            });
        }

        public void SaveToDisk() => CommonApplicationData.SaveToDisk(Constants.FriendlyName, CloneConfigurations());

        private void LoadFromDisk()
        {
            Collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");
                CommonApplicationData.LoadFromDisk<List<NtTunnelInboundConfiguration>>(Constants.FriendlyName)?.ForEach(o => Add(o));
            });
        }
    }
}
