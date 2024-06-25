using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryDistributeUpsertEndpoint : IRmQuery<UIQueryDistributeUpsertEndpointReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public EndpointConfiguration Configuration { get; set; }

        public UIQueryDistributeUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration configuration)
        {
            TunnelKey = tunnelKey;
            Configuration = configuration;
        }
    }

    public class UIQueryDistributeUpsertEndpointReply : IRmQueryReply
    {
        public UIQueryDistributeUpsertEndpointReply()
        {
        }
    }
}
