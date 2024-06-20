using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryGetServiceConfiguration : IRmQuery<QueryGetServiceConfigurationReply>
    {
    }

    public class QueryGetServiceConfigurationReply : IRmQueryReply
    {
        public ServiceConfiguration Configuration { get; set; } = new();

        public QueryGetServiceConfigurationReply()
        {
        }
    }
}
