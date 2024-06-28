using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryDisconnectTunnel : IRmQuery<UIQueryDisconnectTunnelReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public UIQueryDisconnectTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }

    public class UIQueryDisconnectTunnelReply : IRmQueryReply
    {
        public UIQueryDisconnectTunnelReply()
        {
        }
    }
}
