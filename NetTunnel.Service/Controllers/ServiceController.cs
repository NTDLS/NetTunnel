using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine;
using Newtonsoft.Json;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        public ServiceController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/GetStatistics")]
        public NtActionResponseStatistics GetStatistics(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var stats = new NtActionResponseStatistics()
                {
                    Success = true
                };

                stats.Statistics.AddRange(Singletons.Core.InboundTunnels.GetStatistics());
                stats.Statistics.AddRange(Singletons.Core.OutboundTunnels.GetStatistics());

                return stats;
            }
            catch (Exception ex)
            {
                return new NtActionResponseStatistics(ex);
            }
        }
    }
}
