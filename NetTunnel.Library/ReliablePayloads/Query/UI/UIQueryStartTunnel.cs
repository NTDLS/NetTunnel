using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryStartTunnel : IRmQuery<UIQueryStartTunnelReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public UIQueryStartTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }

    public class UIQueryStartTunnelReply : IRmQueryReply
    {
        public UIQueryStartTunnelReply()
        {
        }
    }
}
