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

        [HttpGet]
        [Route("{sessionId}/GetConfiguration")]
        public NtActionResponseServiceConfiguration GetConfiguration(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseServiceConfiguration(Singletons.Configuration);
            }
            catch (Exception ex)
            {
                return new NtActionResponseServiceConfiguration(ex);
            }
        }

        [HttpPost]
        [Route("{sessionId}/Add")]
        public NtActionResponse Add(Guid sessionId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var tunnel = JsonConvert.DeserializeObject<NtServiceConfiguration>(value).EnsureNotNull();

                //Singletons.Core.InboundTunnels.Add(tunnel);
                //Singletons.Core.InboundTunnels.SaveToDisk();

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
    }
}
