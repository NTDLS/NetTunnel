using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Types;
using NetTunnel.Service.Engine;
using NetTunnel.Service.Packetizer.PacketPayloads;
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
        [Route("{sessionId}/Delete/{tunnelPairId}")]
        public NtActionResponse Delete(Guid sessionId, Guid tunnelPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.InboundTunnels.Delete(tunnelPairId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
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

                Singletons.Core.InboundTunnels.Start(tunnelPairId);

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

                Singletons.Core.InboundTunnels.Stop(tunnelPairId);

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

                var tunnel = JsonConvert.DeserializeObject<NtTunnelInboundConfiguration>(value);
                Utility.EnsureNotNull(tunnel);

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

                var endpoints = JsonConvert.DeserializeObject<NtEndpointPairConfiguration>(value);
                Utility.EnsureNotNull(endpoints);

                //Add the inbound endpoint to the local tunnel.
                Singletons.Core.InboundTunnels.AddEndpointInbound(tunnelId, endpoints.Inbound);
                Singletons.Core.InboundTunnels.SaveToDisk();

                var result = await Singletons.Core.InboundTunnels.DispatchAddEndpointOutbound<NtPacketPayloadBoolean>(tunnelId, endpoints.Outbound);

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

                var result = await Singletons.Core.OutboundTunnels.DispatchAddEndpointOutbound<NtPacketPayloadBoolean>(tunnelId, endpoints.Outbound);

                return new NtActionResponse { Success = result?.Value ?? false };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
