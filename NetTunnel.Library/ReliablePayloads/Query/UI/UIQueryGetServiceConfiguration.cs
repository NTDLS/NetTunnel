using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryGetServiceConfiguration : IRmQuery<UIQueryGetServiceConfigurationReply>
    {
    }

    public class UIQueryGetServiceConfigurationReply : IRmQueryReply
    {
        public ServiceConfiguration Configuration { get; set; } = new();

        public UIQueryGetServiceConfigurationReply()
        {
        }
    }
}
