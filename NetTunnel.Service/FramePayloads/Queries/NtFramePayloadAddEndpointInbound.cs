using NetTunnel.Library.Types;
using NetTunnel.Service.FramePayloads.Replies;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadAddEndpointInbound : IRmQuery<NtFramePayloadBoolean>
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public NtEndpointInboundConfiguration Configuration { get; set; } = new();

        public NtFramePayloadAddEndpointInbound() { }

        public NtFramePayloadAddEndpointInbound(NtEndpointInboundConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
