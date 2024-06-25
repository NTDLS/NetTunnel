using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryPutServiceConfiguration : IRmQuery<UIQueryPutServiceConfigurationReply>
    {
        public ServiceConfiguration Configuration { get; set; }

        public UIQueryPutServiceConfiguration(ServiceConfiguration configuration)
        {
            Configuration = configuration;

        }
    }

    public class UIQueryPutServiceConfigurationReply : IRmQueryReply
    {
        public UIQueryPutServiceConfigurationReply()
        {
        }
    }
}
