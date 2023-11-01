using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Replies
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadKeyExchangeReply : INtFramePayloadReply
    {
        [ProtoMember(1)]
        public byte[] NegotiationToken { get; set; }

        public NtFramePayloadKeyExchangeReply()
        {
            NegotiationToken = new byte[0];
        }

        public NtFramePayloadKeyExchangeReply(byte[] negotiationToken)
        {
            NegotiationToken = negotiationToken;
        }
    }
}
