using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryUpsertEndpoint : IRmQuery<QueryUpsertEndpointReply>
    {
        public Guid TunnelId { get; set; }
        public EndpointConfiguration Configuration { get; set; }

        public QueryUpsertEndpoint(Guid tunnelId, EndpointConfiguration configuration)
        {
            TunnelId = tunnelId;
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
