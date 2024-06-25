using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UIOrService
{
    public class UOSQueryRequestKeyExchange : IRmQuery<UOSQueryReplyKeyExchangeReply>
    {
        public byte[] NegotiationToken { get; set; }

        public UOSQueryRequestKeyExchange()
        {
            NegotiationToken = new byte[0];
        }

        public UOSQueryRequestKeyExchange(byte[] negotiationToken)
        {
            NegotiationToken = negotiationToken;
        }
    }

    public class UOSQueryReplyKeyExchangeReply : IRmQueryReply
    {
        public Guid ConnectionId { get; set; }
        public byte[] NegotiationToken { get; set; }

        public UOSQueryReplyKeyExchangeReply()
        {
            NegotiationToken = new byte[0];
        }

        public UOSQueryReplyKeyExchangeReply(Guid connectionId, byte[] negotiationToken)
        {
            ConnectionId = connectionId;
            NegotiationToken = negotiationToken;
        }
    }
}
