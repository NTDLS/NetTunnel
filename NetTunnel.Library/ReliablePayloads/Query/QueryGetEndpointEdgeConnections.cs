using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryGetEndpointEdgeConnections : IRmQuery<QueryGetEndpointEdgeConnectionsReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public DirectionalKey EndpointKey { get; set; }

        public QueryGetEndpointEdgeConnections(DirectionalKey tunnelKey, DirectionalKey endpointKey)
        {
            TunnelKey = tunnelKey;
            EndpointKey = endpointKey;
        }

        public QueryGetEndpointEdgeConnections()
        {
            TunnelKey = new();
            EndpointKey = new();
        }
    }

    public class QueryGetEndpointEdgeConnectionsReply : IRmQueryReply
    {
        public List<EndpointEdgeConnectionDisplay> Collection { get; set; } = new();

        public QueryGetEndpointEdgeConnectionsReply()
        {
        }
    }
}

