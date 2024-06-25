using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryGetEndpointProperties : IRmQuery<UIQueryGetEndpointPropertiesReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public DirectionalKey EndpointKey { get; set; }

        public UIQueryGetEndpointProperties(DirectionalKey tunnelKey, DirectionalKey endpointKey)
        {
            TunnelKey = tunnelKey;
            EndpointKey = endpointKey;
        }

        public UIQueryGetEndpointProperties()
        {
            TunnelKey = new();
            EndpointKey = new();
        }
    }

    public class UIQueryGetEndpointPropertiesReply : IRmQueryReply
    {
        public EndpointPropertiesDisplay Properties { get; set; } = new();

        public UIQueryGetEndpointPropertiesReply()
        {
        }
    }
}

