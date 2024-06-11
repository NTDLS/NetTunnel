using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Notification
{
    [Serializable]
    [ProtoContract]
    public class NotificationEndpointExchange : IRmNotification
    {
        [ProtoMember(1)]
        public Guid StreamId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelId { get; set; }

        [ProtoMember(3)]
        public Guid EndpointId { get; set; }

        [ProtoMember(4)]
        public byte[] Bytes { get; set; }

        public NotificationEndpointExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
        {
            StreamId = streamId;
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Bytes = new byte[length];

            Array.Copy(bytes, Bytes, length);
        }

        public NotificationEndpointExchange()
        {
            Bytes = Array.Empty<byte>();
        }
    }
}
