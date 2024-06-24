using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryGetEndpointProperties : IRmQuery<QueryGetEndpointPropertiesReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public DirectionalKey EndpointKey { get; set; }

        public QueryGetEndpointProperties(DirectionalKey tunnelKey, DirectionalKey endpointKey)
        {
            TunnelKey = tunnelKey;
            EndpointKey = endpointKey;
        }

        public QueryGetEndpointProperties()
        {
            TunnelKey = new();
            EndpointKey = new();
        }
    }

    public class QueryGetEndpointPropertiesReply : IRmQueryReply
    {
        public EndpointProperties Properties { get; set; } = new();

        public QueryGetEndpointPropertiesReply()
        {
        }
    }
}

