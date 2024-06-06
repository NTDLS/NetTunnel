using NetTunnel.Service.FramePayloads.Replies;
using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadDeleteEndpoint : IRmQuery<NtFramePayloadBoolean>
    {
        [ProtoMember(1)]
        public string Label { get; set; } = string.Empty;
        [ProtoMember(2)]
        public Guid EndpointId { get; set; } = new();

        public NtFramePayloadDeleteEndpoint() { }

        public NtFramePayloadDeleteEndpoint(Guid endpointId)
        {
            EndpointId = endpointId;
        }
    }
}
