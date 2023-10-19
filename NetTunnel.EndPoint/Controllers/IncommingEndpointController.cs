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
    public class IncommingEndpointController : ControllerBase
    {
        public IncommingEndpointController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseIncommingEndpoints List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseIncommingEndpoints
                {
                    Collection = Singletons.Core.IncommingEndpoints.Clone(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseIncommingEndpoints(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Delete/{endpointId}")]
        public NtActionResponse Delete(Guid sessionId, Guid endpointId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.IncommingEndpoints.Delete(endpointId);
                Singletons.Core.IncommingEndpoints.SaveToDisk();

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

                var endpoint = JsonConvert.DeserializeObject<NtIncommingEndpoint>(value);
                Utility.EnsureNotNull(endpoint);

                Singletons.Core.IncommingEndpoints.Add(endpoint);
                Singletons.Core.IncommingEndpoints.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
