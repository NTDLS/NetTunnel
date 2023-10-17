using Microsoft.AspNetCore.Mvc;
using NetTunnel.Library.Payloads;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConfigurationController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("Login/{username}/{passwordHash}")]
        public ActionResponse Login(string username, string passwordHash)
        {
            try
            {
                Singletons.Core.Log.Write($"Login: Username: {username}");

                var clientIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var userSession = Singletons.Core.Sessions.Login(username, passwordHash, clientIpAddress);

                if (userSession != null)
                {
                    Singletons.Core.Log.Write($"Login success: Username: {username}, Session: {userSession.SessionId}");
                    return new ActionResponseLogin(userSession.SessionId) { Success = false };
                }
                else
                {
                    Singletons.Core.Log.Write($"Login failed: Username: {username}");
                    throw new Exception("Login failed.");
                }
            }
            catch (Exception ex)
            {
                Singletons.Core.Log.Write($"Login exception: Username: {username}, Exception: {ex.Message}");
                return new ActionResponseLogin(ex)
                {
                    ExceptionText = ex.Message,
                    Success = false
                };
            }
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public ActionResponse ListEndpoints(Guid sessionId)
        {
            try
            {
                Singletons.Core.Log.Write($"ListEndpoints: SessionId: {sessionId}");

                var clientIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var userSession = Singletons.Core.Sessions.Acquire(sessionId, clientIpAddress);

                return new ActionResponse
                {
                    Success = false
                };
            }
            catch (Exception ex)
            {
                Singletons.Core.Log.Write($"ListEndpoints Exception: {ex.Message}");

                return new ActionResponse
                {
                    ExceptionText = ex.Message,
                    Success = false
                };
            }
        }
    }
}
