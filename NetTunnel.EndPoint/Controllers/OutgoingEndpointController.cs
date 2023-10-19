using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.EndPoint.Engine;
using NetTunnel.Library.Types;
using Newtonsoft.Json;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OutgoingEndpointController : ControllerBase
    {
        public OutgoingEndpointController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseOutgoingEndpoints List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseOutgoingEndpoints
                {
                    Collection = Singletons.Core.OutgoingEndpoints.Clone(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseOutgoingEndpoints(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Delete/{endpointId}")]
        public NtActionResponse Delete(Guid sessionId, Guid endpointId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.OutgoingEndpoints.Delete(endpointId);
                Singletons.Core.OutgoingEndpoints.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponseIncommingEndpoints(ex);
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

                var endpoint = JsonConvert.DeserializeObject<NtOutgoingEndpoint>(value);
                Utility.EnsureNotNull(endpoint);

                Singletons.Core.OutgoingEndpoints.Add(endpoint);
                Singletons.Core.OutgoingEndpoints.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
