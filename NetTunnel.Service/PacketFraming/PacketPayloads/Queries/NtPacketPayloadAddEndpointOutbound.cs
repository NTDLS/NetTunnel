using NetTunnel.Library.Types;
using ProtoBuf;

namespace NetTunnel.Service.PacketFraming.PacketPayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadAddEndpointOutbound : IPacketPayloadQuery
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public NtEndpointOutboundConfiguration Configuration { get; set; } = new();

        public NtPacketPayloadAddEndpointOutbound() { }

        public NtPacketPayloadAddEndpointOutbound(NtEndpointOutboundConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}
