using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryDeleteTunnel : IRmQuery<UIQueryDeleteTunnelReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public UIQueryDeleteTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }

    public class UIQueryDeleteTunnelReply : IRmQueryReply
    {
        public UIQueryDeleteTunnelReply()
        {
        }
    }
}
