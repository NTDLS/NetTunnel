using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query.Reply
{
    [Serializable]
    [ProtoContract]
    public class oldQueryReplyKeyExchangeReply : IRmQueryReply
    {
        [ProtoMember(1)]
        public byte[] NegotiationToken { get; set; }

        public oldQueryReplyKeyExchangeReply()
        {
            NegotiationToken = new byte[0];
        }

        public oldQueryReplyKeyExchangeReply(byte[] negotiationToken)
        {
            NegotiationToken = negotiationToken;
        }
    }
}
