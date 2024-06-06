using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Replies
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadKeyExchangeReply : IRmQueryReply
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
