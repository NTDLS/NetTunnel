using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query.Reply
{
    [Serializable]
    [ProtoContract]
    public class QueryReplyKeyExchangeReply : IRmQueryReply
    {
        [ProtoMember(1)]
        public byte[] NegotiationToken { get; set; }

        public QueryReplyKeyExchangeReply()
        {
            NegotiationToken = new byte[0];
        }

        public QueryReplyKeyExchangeReply(byte[] negotiationToken)
        {
            NegotiationToken = negotiationToken;
        }
    }
}
