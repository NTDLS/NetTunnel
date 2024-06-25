using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.ServiceToService
{
    public class S2SQueryUpsertEndpoint : IRmQuery<S2SQueryUpsertEndpointReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public EndpointConfiguration Configuration { get; set; }

        public S2SQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
        {
            TunnelKey = tunnelKey;
            Configuration = configuration;
        }
    }

    public class S2SQueryUpsertEndpointReply : IRmQueryReply
    {
        public S2SQueryUpsertEndpointReply()
        {
        }
    }
}
