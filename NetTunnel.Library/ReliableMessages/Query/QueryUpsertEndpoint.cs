using NetTunnel.Library.Types;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliableMessages.Query
{
    public class QueryUpsertEndpoint : IRmQuery<QueryUpsertEndpointReply>
    {
        public NtEndpointConfiguration Configuration { get; set; }

        public QueryUpsertEndpoint(NtEndpointConfiguration configuration)
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
