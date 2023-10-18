using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload.Response;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EndpointController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EndpointController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseEndpoints List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Log.Write($"ListEndpoints: SessionId: {sessionId}");

                var clientIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var userSession = Singletons.Core.Sessions.Acquire(sessionId, clientIpAddress);


                return new NtActionResponseEndpoints
                {
                    Collection = Singletons.Core.Endpoints.Clone(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                Singletons.Core.Log.Write($"ListEndpoints Exception: {ex.Message}");

                return new NtActionResponseEndpoints
                {
                    ExceptionText = ex.Message,
                    Success = false
                };
            }
        }
    }
}
