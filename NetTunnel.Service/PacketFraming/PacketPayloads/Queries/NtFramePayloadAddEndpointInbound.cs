using NetTunnel.Library.Types;
using ProtoBuf;

namespace NetTunnel.Service.PacketFraming.PacketPayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadAddEndpointInbound : INtFramePayloadQuery
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
