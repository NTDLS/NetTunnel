using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library.Types;
using NetTunnel.Service.Engine;
using Newtonsoft.Json;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OutboundTunnelController : ControllerBase
    {
        public OutboundTunnelController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseOutboundTunnels List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseOutboundTunnels
                {
                    Collection = Singletons.Core.OutgoingTunnels.CloneConfigurations(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseOutboundTunnels(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Delete/{endpointId}")]
        public NtActionResponse Delete(Guid sessionId, Guid endpointId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.OutgoingTunnels.Delete(endpointId);
                Singletons.Core.OutgoingTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseInboundTunnels(ex);
            }
        }

        /// <summary>
        /// This is called locally to add a local listening endpoint. This is the endpoint that may be behind a firewall.
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

                var endpoint = JsonConvert.DeserializeObject<NtTunnelOutboundConfig>(value);
                Utility.EnsureNotNull(endpoint);

                Singletons.Core.OutgoingTunnels.Add(endpoint);
                Singletons.Core.OutgoingTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
