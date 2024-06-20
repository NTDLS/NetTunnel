using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryDeleteEndpoint : IRmQuery<QueryDeleteEndpointReply>
    {
        public Guid TunnelId { get; set; }
        public Guid EndpointId { get; set; }

        public QueryDeleteEndpoint(Guid tunnelId, Guid endpointId)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
        }

        public QueryDeleteEndpoint()
        {

        }
    }

    public class QueryDeleteEndpointReply : IRmQueryReply
    {
        public QueryDeleteEndpointReply()
        {
        }
    }
}
