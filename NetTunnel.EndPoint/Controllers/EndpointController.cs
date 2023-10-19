using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EndpointController : ControllerBase
    {
        public EndpointController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseEndpoints List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseEndpoints
                {
                    Collection = Singletons.Core.Endpoints.Clone(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseEndpoints(ex);
            }
        }
    }
}
