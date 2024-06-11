﻿using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library;
using NetTunnel.Service.TunnelEngine;
using Newtonsoft.Json;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController(IHttpContextAccessor httpContextAccessor)
        : ControllerBase(httpContextAccessor)
    {
        [HttpGet]
        [Route("{sessionId}/ListUsers")]
        public NtActionResponseUsers ListUsers(Guid sessionId)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

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
                ValidateAndEnforceLoginSession(sessionId);

                var content = JsonConvert.DeserializeObject<NtUser>(value).EnsureNotNull();

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

        [HttpPost]
        [Route("{sessionId}/ChangeUserPassword")]
        public NtActionResponse ChangeUserPassword(Guid sessionId, [FromBody] string value)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                var content = JsonConvert.DeserializeObject<NtUser>(value).EnsureNotNull();

                Singletons.Core.Users.ChangePassword(content);
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

        [HttpPost]
        [Route("{sessionId}/DeleteUser")]
        public NtActionResponse DeleteUser(Guid sessionId, [FromBody] string value)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                var content = JsonConvert.DeserializeObject<NtUser>(value).EnsureNotNull();

                Singletons.Core.Users.Delete(content);
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
                var userSession = ValidateAndEnforceLoginSession(sessionId);

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
