using NetTunnel.Library.Types;
using ProtoBuf;

namespace NetTunnel.Service.Packetizer.PacketPayloads
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadAddEndpointInbound : IPacketPayload
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public NtEndpointInboundConfiguration Configuration { get; set; } = new();

        public NtPacketPayloadAddEndpointInbound() { }

        public NtPacketPayloadAddEndpointInbound(NtEndpointInboundConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
