using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryUpsertEndpoint : IRmQuery<QueryUpsertEndpointReply>
    {
        public Guid TunnelId { get; set; }
        public NtEndpointConfiguration Configuration { get; set; }

        public QueryUpsertEndpoint(Guid TunnelId, NtEndpointConfiguration configuration)
        {
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
