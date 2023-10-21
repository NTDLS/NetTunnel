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
    public class InboundEndpointController : ControllerBase
    {
        public InboundEndpointController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List/{tunnelPairId}")]
        public NtActionResponseEndpointsInbound List(Guid sessionId, Guid tunnelPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var tunnel = Singletons.Core.InboundTunnels.CloneConfiguration(tunnelPairId);

                return new NtActionResponseEndpointsInbound
                {
                    Collection = tunnel.InboundEndpointConfigurations,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseEndpointsInbound(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Delete/{tunnelPairId}/{endpointPairId}")]
        public NtActionResponse Delete(Guid sessionId, Guid tunnelPairId, Guid endpointPairId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var tunnel = Singletons.Core.InboundTunnels.CloneConfiguration(tunnelPairId);
                Singletons.Core.InboundTunnels.DeleteInboundEndpoint(tunnelPairId, endpointPairId);
                Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseTunnelsInbound(ex);
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

                var endpoint = JsonConvert.DeserializeObject<NtEndpointInboundConfiguration>(value);
                Utility.EnsureNotNull(endpoint);

                Singletons.Core.InboundTunnels.AddInboundEndpoint(tunnelPairId, endpoint);
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
