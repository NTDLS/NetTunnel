﻿using NTDLS.ReliableMessaging;
using ProtoBuf;

namespace NetTunnel.Service.ReliableMessages.Notification
{
    [Serializable]
    [ProtoContract]
    public class oldNotificationEndpointConnect : IRmNotification
    {
        [ProtoMember(1)]
        public Guid StreamId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelId { get; set; }

        [ProtoMember(3)]
        public Guid EndpointId { get; set; }


        public oldNotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
        {
            StreamId = streamId;
            TunnelId = tunnelId;
            EndpointId = endpointId;
        }

        public oldNotificationEndpointConnect()
        {
        }
    }
}