using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Query
{
    [Serializable]
    [ProtoContract]
    public class oldQueryRequestKeyExchange : IRmQuery<oldQueryReplyKeyExchangeReply>
    {
        [ProtoMember(1)]
        public byte[] NegotiationToken { get; set; }

        public oldQueryRequestKeyExchange()
        {
            NegotiationToken = new byte[0];
        }

        public oldQueryRequestKeyExchange(byte[] negotiationToken)
        {
            NegotiationToken = negotiationToken;
        }
    }
}
