using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadEndpointConnect : INtFramePayloadNotification
    {
        [ProtoMember(1)]
        public Guid StreamId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelPairId { get; set; }

        [ProtoMember(3)]
        public Guid EndpointPairId { get; set; }


        public NtFramePayloadEndpointConnect(Guid tunnelPairId, Guid endpointPairId, Guid streamId)
        {
            StreamId = streamId;
            TunnelPairId = tunnelPairId;
            EndpointPairId = endpointPairId;
        }

        public NtFramePayloadEndpointConnect()
        {
        }
    }
}
