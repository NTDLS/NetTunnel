using Microsoft.AspNetCore.Mvc;

namespace NetTunnel.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController(IHttpContextAccessor httpContextAccessor)
        : ControllerBase(httpContextAccessor)
    {
        /*
        [HttpGet]
        [Route("{sessionId}/GetStatistics")]
        public NtActionResponseStatistics GetStatistics(Guid sessionId)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

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
                ValidateAndEnforceLoginSession(sessionId);

                return new NtActionResponseServiceConfiguration(Singletons.Configuration);
            }
            catch (Exception ex)
            {
                return new NtActionResponseServiceConfiguration(ex);
            }
        }

        [HttpPost]
        [Route("{sessionId}/PutConfiguration")]
        public NtActionResponse PutConfiguration(Guid sessionId, [FromBody] string value)
        {
            try
            {
                ValidateAndEnforceLoginSession(sessionId);

                var configuration = JsonConvert.DeserializeObject<NtServiceConfiguration>(value).EnsureNotNull();

                Singletons.UpdateConfiguration(configuration);

                return new NtActionResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new NtActionResponse(ex);
            }
        }
        */
    }
}
