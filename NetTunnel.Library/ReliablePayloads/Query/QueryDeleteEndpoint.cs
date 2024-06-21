using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryDeleteEndpoint : IRmQuery<QueryDeleteEndpointReply>
    {
        public DirectionalKey TunnelKey { get; set; }
        public Guid EndpointId { get; set; }

        public QueryDeleteEndpoint(DirectionalKey tunnelKey, Guid endpointId)
        {
            TunnelKey = tunnelKey;
            EndpointId = endpointId;
        }
    }

    public class QueryDeleteEndpointReply : IRmQueryReply
    {
        public QueryDeleteEndpointReply()
        {
        }
    }
}
