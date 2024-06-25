using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryGetTunnels : IRmQuery<UIQueryGetTunnelsReply>
    {
    }

    public class UIQueryGetTunnelsReply : IRmQueryReply
    {
        public List<TunnelDisplay> Collection { get; set; } = new();

        public UIQueryGetTunnelsReply()
        {
        }
    }
}
