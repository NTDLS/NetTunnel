using Microsoft.AspNetCore.Mvc;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TunnelInboundController(IHttpContextAccessor httpContextAccessor)
        : ControllerBase(httpContextAccessor)
    {
        /*
        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseTunnelsInbound List(Guid sessionId)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

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
                ValidateAndEnforceLoginSession(sessionId);

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
                ValidateAndEnforceLoginSession(sessionId);

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
                ValidateAndEnforceLoginSession(sessionId);

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
                ValidateAndEnforceLoginSession(sessionId);

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
                ValidateAndEnforceLoginSession(sessionId);

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
        [Route("{sessionId}/UpsertEndpointInboundPair/{tunnelId}")]
        public async Task<NtActionResponse> UpsertEndpointInboundPair(Guid sessionId, Guid tunnelId, [FromBody] string value)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                var endpoint = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value).EnsureNotNull();

                //Add the inbound endpoint to the local tunnel.
                Singletons.Core.InboundTunnels.UpsertEndpointInbound(tunnelId, endpoint.Inbound);
                Singletons.Core.InboundTunnels.SaveToDisk();

                //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
                var result = await Singletons.Core.InboundTunnels
                    .DispatchUpsertEndpointOutboundToAssociatedTunnelService<oldQueryReplyPayloadBoolean>(tunnelId, endpoint.Outbound);

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
                Singletons.Core.InboundTunnels.UpsertEndpointOutbound(tunnelId, endpoint.Outbound);
                Singletons.Core.InboundTunnels.SaveToDisk();

                //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
                var result = await Singletons.Core.InboundTunnels
                    .DispatchUpsertEndpointInboundToAssociatedTunnelService<oldQueryReplyPayloadBoolean>(tunnelId, endpoint.Inbound);

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
                Singletons.Core.InboundTunnels.DeleteEndpoint(tunnelId, endpointId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                //Since we have a tunnel, we will communicate the alteration of endpoints though the tunnel.
                var result = await Singletons.Core.InboundTunnels
                    .DispatchDeleteEndpointToAssociatedTunnelService<oldQueryReplyPayloadBoolean>(tunnelId, endpointId);

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
                Singletons.Core.InboundTunnels.DeleteEndpoint(tunnelId, endpointId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
        */
    }
}
