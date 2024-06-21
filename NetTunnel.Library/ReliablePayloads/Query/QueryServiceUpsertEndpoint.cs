using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryDistributeUpsertEndpoint : IRmQuery<QueryDistributeUpsertEndpointReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public EndpointConfiguration Configuration { get; set; }

        public QueryDistributeUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
        {
            TunnelKey = tunnelKey;
            Configuration = configuration;
        }
    }

    public class QueryDistributeUpsertEndpointReply : IRmQueryReply
    {
        public QueryDistributeUpsertEndpointReply()
        {
        }
    }
}
