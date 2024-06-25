using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryGetEndpointEdgeConnections : IRmQuery<UIQueryGetEndpointEdgeConnectionsReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public DirectionalKey EndpointKey { get; set; }

        public UIQueryGetEndpointEdgeConnections(DirectionalKey tunnelKey, DirectionalKey endpointKey)
        {
            TunnelKey = tunnelKey;
            EndpointKey = endpointKey;
        }

        public UIQueryGetEndpointEdgeConnections()
        {
            TunnelKey = new();
            EndpointKey = new();
        }
    }

    public class UIQueryGetEndpointEdgeConnectionsReply : IRmQueryReply
    {
        public List<EndpointEdgeConnectionDisplay> Collection { get; set; } = new();

        public UIQueryGetEndpointEdgeConnectionsReply()
        {
        }
    }
}

