using Microsoft.AspNetCore.Mvc;
using NetTunnel.ClientAPI;
using NetTunnel.ClientAPI.Payload;
using NetTunnel.EndPoint.Engine;
using NetTunnel.Library.Types;
using Newtonsoft.Json;

namespace NetTunnel.EndPoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EndpointController : ControllerBase
    {
        public EndpointController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet]
        [Route("{sessionId}/List")]
        public NtActionResponseEndpoints List(Guid sessionId)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                return new NtActionResponseEndpoints
                {
                    Collection = Singletons.Core.Endpoints.Clone(),
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new NtActionResponseEndpoints(ex);
            }
        }

        /// <summary>
        /// This is called locally to add a local listening endpoint. This is the endpoint that may be behind a firewall.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{sessionId}/AddOutgoing")]
        public NtActionResponse AddOutgoing(Guid sessionId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var endpoint = JsonConvert.DeserializeObject<NtEndpoint>(value);
                Utility.EnsureNotNull(endpoint);

                endpoint.Direction = Library.Constants.EndpointDirection.Outgoing;

                Singletons.Core.Endpoints.Add(endpoint);
                Singletons.Core.Endpoints.SaveToDisk();

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

        /// <summary>
        /// This is called from a remote endpoint to tell us to setup the connection.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{sessionId}/AddIncomming")]
        public NtActionResponse AddIncomming(Guid sessionId, [FromBody] string value)
        {
            try
            {
                Singletons.Core.Sessions.Validate(sessionId, GetPeerIpAddress());

                var incommingEndpoint = JsonConvert.DeserializeObject<NtIncommingEndpoint>(value);
                Utility.EnsureNotNull(incommingEndpoint);

                incommingEndpoint.Endpoint.Direction = Library.Constants.EndpointDirection.Incomming;

                Singletons.Core.Endpoints.Add(incommingEndpoint.Endpoint);
                Singletons.Core.Endpoints.SaveToDisk();

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
