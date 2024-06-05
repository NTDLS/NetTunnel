using ProtoBuf;

namespace NetTunnel.Service.MessageFraming.FramePayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtFramePayloadEndpointDisconnect : INtFramePayloadNotification
    {
        [ProtoMember(1)]
        public Guid StreamId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelId { get; set; }

        [ProtoMember(3)]
        public Guid EndpointId { get; set; }


        public NtFramePayloadEndpointDisconnect(Guid tunnelId, Guid endpointId, Guid streamId)
        {
            StreamId = streamId;
            TunnelId = tunnelId;
            EndpointId = endpointId;
        }

        public NtFramePayloadEndpointDisconnect()
        {
        }
    }
}
