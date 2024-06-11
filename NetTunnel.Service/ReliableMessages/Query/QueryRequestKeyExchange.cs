using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query
{
    [Serializable]
    [ProtoContract]
    public class QueryRequestKeyExchange : IRmQuery<QueryReplyKeyExchangeReply>
    {
        [ProtoMember(1)]
        public byte[] NegotiationToken { get; set; }

        public QueryRequestKeyExchange()
        {
            NegotiationToken = new byte[0];
        }

        public QueryRequestKeyExchange(byte[] negotiationToken)
        {
            NegotiationToken = negotiationToken;
        }
    }
}
