using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query
{
    [Serializable]
    [ProtoContract]
    public class QueryUpsertEndpointInbound : IRmQuery<QueryReplyPayloadBoolean>
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public NtEndpointInboundConfiguration Configuration { get; set; } = new();

        public QueryUpsertEndpointInbound() { }

        public QueryUpsertEndpointInbound(NtEndpointInboundConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
