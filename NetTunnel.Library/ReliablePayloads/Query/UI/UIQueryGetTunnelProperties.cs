using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryGetTunnelProperties : IRmQuery<UIQueryGetTunnelPropertiesReply>
    {
        public DirectionalKey TunnelKey { get; set; }

        public UIQueryGetTunnelProperties(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }

        public UIQueryGetTunnelProperties()
        {
            TunnelKey = new();
        }
    }

    public class UIQueryGetTunnelPropertiesReply : IRmQueryReply
    {
        public TunnelPropertiesDisplay Properties { get; set; } = new();

        public UIQueryGetTunnelPropertiesReply()
        {
        }
    }
}

