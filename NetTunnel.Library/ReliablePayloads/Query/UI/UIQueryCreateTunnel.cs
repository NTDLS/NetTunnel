using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryCreateTunnel : IRmQuery<UIQueryCreateTunnelReply>
    {
        public TunnelConfiguration Configuration { get; set; }

        public UIQueryCreateTunnel(TunnelConfiguration configuration)
        {
            Configuration = configuration;
        }

    }

    public class UIQueryCreateTunnelReply : IRmQueryReply
    {
        public UIQueryCreateTunnelReply()
        {
        }
    }
}
