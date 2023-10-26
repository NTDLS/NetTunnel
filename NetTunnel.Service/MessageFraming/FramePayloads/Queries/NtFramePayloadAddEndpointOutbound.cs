using NetTunnel.Library.Types;
using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadAddEndpointOutbound : INtFramePayloadQuery
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
