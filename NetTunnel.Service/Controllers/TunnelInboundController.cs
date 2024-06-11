using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NetTunnel.Service.TunnelEngine;
using Newtonsoft.Json;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TunnelInboundController : ControllerBase
    {
        public TunnelInboundController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseTunnelsInbound List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseTunnelsInbound
                {
                    Collection = Singletons.Core.InboundTunnels.CloneConfigurations(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Delete/{tunnelId}")]
        public NtActionResponse Delete(Guid sessionId, Guid tunnelId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.InboundTunnels.Delete(tunnelId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/DeletePair/{tunnelId}")]
        public NtActionResponse DeletePair(Guid sessionId, Guid tunnelId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.InboundTunnels.DeletePair(tunnelId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Start/{tunnelId}")]
        public NtActionResponse Start(Guid sessionId, Guid tunnelId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.InboundTunnels.Start(tunnelId);

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Stop/{tunnelId}")]
        public NtActionResponse Stop(Guid sessionId, Guid tunnelId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.InboundTunnels.Stop(tunnelId);

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        /// <summary>
        /// This is called locally to add a local listening tunnel. This is the tunnel that may be behind a firewall.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{sessionId}/Add")]
        public NtActionResponse Add(Guid sessionId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var tunnel = JsonConvert.DeserializeObject<NtTunnelInboundConfiguration>(value).EnsureNotNull();

                Singletons.Core.InboundTunnels.Add(tunnel);
                Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }

        [HttpPost]
        [Route("{sessionId}/AddEndpointInboundPair/{tunnelId}")]
        public async Task<NtActionResponse> AddEndpointInboundPair(Guid sessionId, Guid tunnelId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var endpoints = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value).EnsureNotNull();

                //Add the inbound endpoint to the local tunnel.
                Singletons.Core.InboundTunnels.AddEndpointInbound(tunnelId, endpoints.Inbound);
                Singletons.Core.InboundTunnels.SaveToDisk();

                var result = await Singletons.Core.InboundTunnels.DispatchAddEndpointOutboundToAssociatedTunnelService<QueryReplyPayloadBoolean>(tunnelId, endpoints.Outbound);

                return new NtActionResponse { Success = result?.Value ?? false };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }

        [HttpPost]
        [Route("{sessionId}/AddEndpointOutboundPair/{tunnelId}")]
        public async Task<NtActionResponse> AddEndpointOutboundPair(Guid sessionId, Guid tunnelId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var endpoints = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value).EnsureNotNull();

                //Add the Outbound endpoint to the local tunnel.
                Singletons.Core.InboundTunnels.AddEndpointOutbound(tunnelId, endpoints.Outbound);
                Singletons.Core.InboundTunnels.SaveToDisk();

                //Tell the remote tunnel service to add the endpoint.
                var result = await Singletons.Core.InboundTunnels.DispatchAddEndpointInboundToAssociatedTunnelService<QueryReplyPayloadBoolean>(tunnelId, endpoints.Inbound);

                return new NtActionResponse { Success = result?.Value ?? false };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/DeleteEndpointPair/{tunnelId}/{endpointId}")]
        public async Task<NtActionResponse> DeleteEndpointPair(Guid sessionId, Guid tunnelId, Guid endpointId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                //Remove the the endpoint to the local tunnel.
                Singletons.Core.InboundTunnels.DeleteEndpoint(tunnelId, endpointId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                //Tell the remote tunnel service to delete the endpoint.
                var result = await Singletons.Core.InboundTunnels.DispatchDeleteEndpointToAssociatedTunnelService<QueryReplyPayloadBoolean>(tunnelId, endpointId);

                return new NtActionResponse { Success = result?.Value ?? false };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/DeleteEndpoint/{tunnelId}/{endpointId}")]
        public NtActionResponse DeleteEndpoint(Guid sessionId, Guid tunnelId, Guid endpointId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                //Remove the the endpoint to the local tunnel.
                Singletons.Core.InboundTunnels.DeleteEndpoint(tunnelId, endpointId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
