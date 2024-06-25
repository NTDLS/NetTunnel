using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryDeleteEndpoint : IRmQuery<UIQueryDeleteEndpointReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public UIQueryDeleteEndpoint(DirectionalKey tunnelKey, Guid endpointId)
        {
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }
    }

    public class UIQueryDeleteEndpointReply : IRmQueryReply
    {
        public UIQueryDeleteEndpointReply()
        {
        }
    }
}
