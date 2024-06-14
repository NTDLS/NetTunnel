namespace NetTunnel.Service.Controllers
{
    public class ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ControllerBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /*
        /// <summary>
        /// Validates and enforces a login session by throwing an exception if the passed
        /// session id does not exist or does not match the IP address of the peer.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public NtUserSession ValidateAndEnforceLoginSession(Guid sessionId)
            => Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

        public string GetPeerIpAddress()
            => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
        */
    }
}
