using NetTunnel.Library.Types;
using NetTunnel.Service.FramePayloads.Replies;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadAddEndpointOutbound : IRmQuery<NtFramePayloadBoolean>
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public NtEndpointOutboundConfiguration Configuration { get; set; } = new();

        public NtFramePayloadAddEndpointOutbound() { }

        public NtFramePayloadAddEndpointOutbound(NtEndpointOutboundConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
