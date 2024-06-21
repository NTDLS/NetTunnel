using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryRequestKeyExchange : IRmQuery<QueryReplyKeyExchangeReply>
    {
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

    public class QueryReplyKeyExchangeReply : IRmQueryReply
    {
        public Guid ConnectionId { get; set; }
        public byte[] NegotiationToken { get; set; }

        public QueryReplyKeyExchangeReply()
        {
            NegotiationToken = new byte[0];
        }

        public QueryReplyKeyExchangeReply(Guid connectionId, byte[] negotiationToken)
        {
            ConnectionId = connectionId;
            NegotiationToken = negotiationToken;
        }
    }
}
