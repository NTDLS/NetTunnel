using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Types;
using NetTunnel.Service.Engine;
using Newtonsoft.Json;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OutboundEndpointController : ControllerBase
    {
        public OutboundEndpointController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List/{tunnelPairId}")]
        public NtActionResponseEndpointsOutbound List(Guid sessionId, Guid tunnelPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var tunnel = Singletons.Core.OutboundTunnels.CloneConfiguration(tunnelPairId);

                return new NtActionResponseEndpointsOutbound
                {
                    Collection = tunnel.OutboundEndpointConfigurations,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseEndpointsOutbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Delete/{tunnelPairId}/{endpointPairId}")]
        public NtActionResponse Delete(Guid sessionId, Guid tunnelPairId, Guid endpointPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var tunnel = Singletons.Core.OutboundTunnels.CloneConfiguration(tunnelPairId);
                Singletons.Core.OutboundTunnels.DeleteOutboundEndpoint(tunnelPairId, endpointPairId);
                Singletons.Core.OutboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsOutbound(ex);
            }
        }

        /// <summary>
        /// This is called locally to add a local listening endpoint. This is the endpoint that may be behind a firewall.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{sessionId}/Add/{tunnelPairId}")]
        public NtActionResponse Add(Guid sessionId, Guid tunnelPairId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var endpoint = JsonConvert.DeserializeObject<NtEndpointOutboundConfiguration>(value);
                Utility.EnsureNotNull(endpoint);

                Singletons.Core.OutboundTunnels.AddOutboundEndpoint(tunnelPairId, endpoint);
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
