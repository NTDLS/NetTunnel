using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadEndpointExchange : INtFramePayloadNotification
    {
        [ProtoMember(1)]
        public Guid StreamId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelPairId { get; set; }

        [ProtoMember(3)]
        public Guid EndpointPairId { get; set; }

        [ProtoMember(4)]
        public byte[] Bytes { get; set; }

        public NtFramePayloadEndpointExchange(Guid tunnelPairId, Guid endpointPairId, Guid streamId, byte[] bytes, int length)
        {
            StreamId = streamId;
            TunnelPairId = tunnelPairId;
            EndpointPairId = endpointPairId;
            Bytes = new byte[length];

            Array.Copy(bytes, Bytes, length);
        }

        public NtFramePayloadEndpointExchange()
        {
            Bytes = Array.Empty<byte>();
        }
    }
}
