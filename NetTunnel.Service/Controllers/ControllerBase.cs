namespace NetTunnel.EndPoint.Controllers
{

    public class ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ControllerBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetPeerIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
