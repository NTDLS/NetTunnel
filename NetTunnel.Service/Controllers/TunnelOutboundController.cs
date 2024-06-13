using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NetTunnel.Service.TunnelEngine;
using Newtonsoft.Json;
using NTDLS.NullExtensions;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TunnelOutboundController(IHttpContextAccessor httpContextAccessor)
        : ControllerBase(httpContextAccessor)
    {
        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseTunnelsOutbound List(Guid sessionId)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                return new NtActionResponseTunnelsOutbound
                {
                    Collection = Singletons.Core.OutboundTunnels.CloneConfigurations(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsOutbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Delete/{tunnelId}")]
        public NtActionResponse Delete(Guid sessionId, Guid tunnelId)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                Singletons.Core.OutboundTunnels.Delete(tunnelId);
                Singletons.Core.OutboundTunnels.SaveToDisk();

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
                ValidateAndEnforceLoginSession(sessionId);

                Singletons.Core.OutboundTunnels.DeletePair(tunnelId);
                Singletons.Core.OutboundTunnels.SaveToDisk();

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
                ValidateAndEnforceLoginSession(sessionId);

                Singletons.Core.OutboundTunnels.Start(tunnelId);

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
                ValidateAndEnforceLoginSession(sessionId);

                Singletons.Core.OutboundTunnels.Stop(tunnelId);

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
                ValidateAndEnforceLoginSession(sessionId);

                var tunnel = JsonConvert.DeserializeObject<NtTunnelOutboundConfiguration>(value).EnsureNotNull();

                Singletons.Core.OutboundTunnels.Add(tunnel);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }

        [HttpPost]
        [Route("{sessionId}/UpsertEndpointInboundPair/{tunnelId}")]
        public async Task<NtActionResponse> UpsertEndpointInboundPair(Guid sessionId, Guid tunnelId, [FromBody] string value)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                var endpoint = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value).EnsureNotNull();

                //Add the inbound endpoint to the local tunnel.
                Singletons.Core.OutboundTunnels.UpsertEndpointInbound(tunnelId, endpoint.Inbound);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
                var result = await Singletons.Core.OutboundTunnels
                    .DispatchUpsertEndpointOutboundToAssociatedTunnelService<QueryReplyPayloadBoolean>(tunnelId, endpoint.Outbound);

                return new NtActionResponse { Success = result?.Value ?? false };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }

        [HttpPost]
        [Route("{sessionId}/UpsertEndpointOutboundPair/{tunnelId}")]
        public async Task<NtActionResponse> UpsertEndpointOutboundPair(Guid sessionId, Guid tunnelId, [FromBody] string value)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                var endpoint = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value).EnsureNotNull();

                //Add the Outbound endpoint to the local tunnel.
                Singletons.Core.OutboundTunnels.UpsertEndpointOutbound(tunnelId, endpoint.Outbound);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
                var result = await Singletons.Core.OutboundTunnels
                    .DispatchUpsertEndpointInboundToAssociatedTunnelService<QueryReplyPayloadBoolean>(tunnelId, endpoint.Inbound);

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
                ValidateAndEnforceLoginSession(sessionId);

                //Remove the the endpoint to the local tunnel.
                Singletons.Core.OutboundTunnels.DeleteEndpoint(tunnelId, endpointId);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
                var result = await Singletons.Core.OutboundTunnels
                    .DispatchDeleteEndpointToAssociatedTunnelService<QueryReplyPayloadBoolean>(tunnelId, endpointId);

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
                ValidateAndEnforceLoginSession(sessionId);

                //Remove the the endpoint to the local tunnel.
                Singletons.Core.OutboundTunnels.DeleteEndpoint(tunnelId, endpointId);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
