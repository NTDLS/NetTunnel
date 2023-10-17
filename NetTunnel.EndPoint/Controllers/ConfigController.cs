using Microsoft.AspNetCore.Mvc;
using NetTunnel.Library.Payloads;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConfigController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("Login/{username}/{passwordHash}")]
        public ControllerActionResponse Login(string username, string passwordHash)
        {
            try
            {
                Singletons.Core.Log.Write($"Login: Username: {username}");

                var clientIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var userSession = Singletons.Core.Sessions.Login(username, passwordHash, clientIpAddress);

                if (userSession != null)
                {
                    Singletons.Core.Log.Write($"Login success: Username: {username}, Session: {userSession.SessionId}");
                    return new ControllerActionResponse(userSession.SessionId) { Success = false };
                }
                else
                {
                    Singletons.Core.Log.Write($"Login failed: Username: {username}");
                    return new ControllerActionResponse() { Success = false };
                }
            }
            catch (Exception ex)
            {
                Singletons.Core.Log.Write($"Login exception: Username: {username}, Exception: {ex.Message}");
                return new ControllerActionResponse
                {
                    ExceptionText = ex.Message,
                    Success = false
                };
            }
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public ControllerActionResponse ListEndpoints(Guid sessionId)
        {
            try
            {
                Singletons.Core.Log.Write($"ListEndpoints: SessionId: {sessionId}");

                var clientIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var userSession = Singletons.Core.Sessions.Acquire(sessionId, clientIpAddress);

                return new ControllerActionResponse
                {
                    Success = false
                };
            }
            catch (Exception ex)
            {
                Singletons.Core.Log.Write($"ListEndpoints Exception: {ex.Message}");

                return new ControllerActionResponse
                {
                    ExceptionText = ex.Message,
                    Success = false
                };
            }
        }
    }
}
