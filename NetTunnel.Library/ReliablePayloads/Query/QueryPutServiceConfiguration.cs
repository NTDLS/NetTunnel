using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryPutServiceConfiguration : IRmQuery<QueryPutServiceConfigurationReply>
    {
        public ServiceConfiguration Configuration { get; set; }

        public QueryPutServiceConfiguration(ServiceConfiguration configuration)
        {
            Configuration = configuration;

        }
    }

    public class QueryPutServiceConfigurationReply : IRmQueryReply
    {
        public QueryPutServiceConfigurationReply()
        {
        }
    }
}
