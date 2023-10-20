using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Service.Engine;
using Newtonsoft.Json;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        public SecurityController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/ListUsers")]
        public NtActionResponseUsers ListUsers(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseUsers
                {
                    Collection = Singletons.Core.Users.Clone(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseUsers(ex);
            }
        }

        [HttpPost]
        [Route("{sessionId}/CreateUser")]
        public NtActionResponse CreateUser(Guid sessionId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var content = JsonConvert.DeserializeObject<NtUser>(value);
                Utility.EnsureNotNull(content);

                Singletons.Core.Users.Add(content);
                Singletons.Core.Users.SaveToDisk();

                return new NtActionResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }

        [HttpGet]
        [Route("{username}/{passwordHash}/Login")]
        public NtActionResponse Login(string username, string passwordHash)
        {
            try
            {
                var userSession = Singletons.Core.Sessions.Login(username, passwordHash, GetPeerIpAddress());

                if (userSession != null)
                {
                    return new NtActionResponseLogin()
                    {
                        SessionId = userSession.SessionId,
                        Success = true
                    };
                }
                else
                {
                    throw new Exception("Login failed.");
                }
            }
            catch (Exception ex)
            {
                return new NtActionResponseLogin(ex);
            }
        }

        [HttpGet]
        [Route("{sessionId}/Logout")]
        public NtActionResponse Logout(Guid sessionId)
        {
            try
            {
                var userSession = Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                Singletons.Core.Sessions.Logout(userSession);

                return new NtActionResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
