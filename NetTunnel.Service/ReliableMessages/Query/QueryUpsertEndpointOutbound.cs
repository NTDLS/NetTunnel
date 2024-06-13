using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query
{
    [Serializable]
    [ProtoContract]
    public class QueryUpsertEndpointOutbound : IRmQuery<QueryReplyPayloadBoolean>
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public NtEndpointOutboundConfiguration Configuration { get; set; } = new();

        public QueryUpsertEndpointOutbound() { }

        public QueryUpsertEndpointOutbound(NtEndpointOutboundConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
