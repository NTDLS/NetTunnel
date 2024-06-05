using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Queries
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadDeleteEndpoint : INtFramePayloadQuery
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
