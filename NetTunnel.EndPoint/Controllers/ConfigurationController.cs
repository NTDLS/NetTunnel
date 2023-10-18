using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload.Response;

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
        [Route("{username}/{passwordHash}/Login")]
        public NtActionResponse Login(string username, string passwordHash)
        {
            try
            {
                Singletons.Core.Log.Write($"Login: Username: {username}");

                var clientIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var userSession = Singletons.Core.Sessions.Login(username, passwordHash, clientIpAddress);

                if (userSession != null)
                {
                    Singletons.Core.Log.Write($"Login success: Username: {username}, Session: {userSession.SessionId}");
                    return new NtActionResponseLogin()
                    {
                        SessionId = userSession.SessionId,
                        Success = true
                    };
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
                return new NtActionResponseLogin(ex)
                {
                    ExceptionText = ex.Message,
                    Success = false
                };
            }
        }

        [HttpGet]
        [Route("{sessionId}/ListEndpoints")]
        public NtActionResponseEndpoints ListEndpoints(Guid sessionId)
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
