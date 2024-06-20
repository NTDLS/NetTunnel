using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryUpsertEndpoint : IRmQuery<QueryUpsertEndpointReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public EndpointConfiguration Configuration { get; set; }

        public QueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
        {
            TunnelKey = tunnelKey;
            Configuration = configuration;
        }

    }

    public class QueryUpsertEndpointReply : IRmQueryReply
    {
        public QueryUpsertEndpointReply()
        {
        }
    }
}
