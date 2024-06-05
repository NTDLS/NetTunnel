using NetTunnel.Service.FramePayloads.Replies;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadRequestKeyExchange : IRmQuery<NtFramePayloadBoolean>
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
