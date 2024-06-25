using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryStopTunnel : IRmQuery<UIQueryStopTunnelReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public UIQueryStopTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }

    public class UIQueryStopTunnelReply : IRmQueryReply
    {
        public UIQueryStopTunnelReply()
        {
        }
    }
}
