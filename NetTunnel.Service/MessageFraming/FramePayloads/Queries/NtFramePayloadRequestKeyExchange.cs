using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadRequestKeyExchange : INtFramePayloadQuery
    {
        [ProtoMember(1)]
        public byte[] NegotiationToken { get; set; }

        public NtFramePayloadRequestKeyExchange()
        {
            NegotiationToken = new byte[0];
        }

        public NtFramePayloadRequestKeyExchange(byte[] negotiationToken)
        {
            NegotiationToken = negotiationToken;
        }
    }
}
