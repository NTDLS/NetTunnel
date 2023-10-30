using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.MessageFraming.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine;
using Newtonsoft.Json;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TunnelOutboundController : ControllerBase
    {
        public TunnelOutboundController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseTunnelsOutbound List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

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
        [Route("{sessionId}/Delete/{tunnelPairId}")]
        public NtActionResponse Delete(Guid sessionId, Guid tunnelPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.OutboundTunnels.Delete(tunnelPairId);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/DeletePair/{tunnelPairId}")]
        public async Task<NtActionResponse> DeletePair(Guid sessionId, Guid tunnelPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return await Singletons.Core.OutboundTunnels.DeletePair(tunnelPairId).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully && x.Result == true)
                    {
                        Singletons.Core.OutboundTunnels.SaveToDisk();
                    }
                    return new NtActionResponse { Success = x.Result };
                });
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Start/{tunnelPairId}")]
        public NtActionResponse Start(Guid sessionId, Guid tunnelPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.OutboundTunnels.Start(tunnelPairId);

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Stop/{tunnelPairId}")]
        public NtActionResponse Stop(Guid sessionId, Guid tunnelPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.OutboundTunnels.Stop(tunnelPairId);

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

                var tunnel = JsonConvert.DeserializeObject<NtTunnelOutboundConfiguration>(value);
                Utility.EnsureNotNull(tunnel);

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
        [Route("{sessionId}/AddEndpointInboundPair/{tunnelId}")]
        public async Task<NtActionResponse> AddEndpointInboundPair(Guid sessionId, Guid tunnelId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var endpoints = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value);
                Utility.EnsureNotNull(endpoints);

                //Add the inbound endpoint to the local tunnel.
                Singletons.Core.OutboundTunnels.AddEndpointInbound(tunnelId, endpoints.Inbound);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                var result = await Singletons.Core.OutboundTunnels.DispatchAddEndpointOutboundToAssociatedTunnelService<NtFramePayloadBoolean>(tunnelId, endpoints.Outbound);

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

                var endpoints = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value);
                Utility.EnsureNotNull(endpoints);

                //Add the Outbound endpoint to the local tunnel.
                Singletons.Core.OutboundTunnels.AddEndpointOutbound(tunnelId, endpoints.Outbound);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                var result = await Singletons.Core.OutboundTunnels.DispatchAddEndpointInboundToAssociatedTunnelService<NtFramePayloadBoolean>(tunnelId, endpoints.Inbound);

                return new NtActionResponse { Success = result?.Value ?? false };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
