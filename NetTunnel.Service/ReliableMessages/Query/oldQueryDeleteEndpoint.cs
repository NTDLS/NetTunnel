using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query
{
    [Serializable]
    [ProtoContract]
    public class oldQueryDeleteEndpoint : IRmQuery<oldQueryReplyPayloadBoolean>
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public Guid EndpointId { get; set; } = new();

        public oldQueryDeleteEndpoint() { }

        public oldQueryDeleteEndpoint(Guid endpointId)
        {
            EndpointId = endpointId;
        }
    }
}
