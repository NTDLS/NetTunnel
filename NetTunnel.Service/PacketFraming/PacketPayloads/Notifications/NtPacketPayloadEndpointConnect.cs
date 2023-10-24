﻿using ProtoBuf;

namespace NetTunnel.Service.PacketFraming.PacketPayloads.Notifications
{
    [Serializable]
    [ProtoContract]
    public class NtPacketPayloadEndpointConnect : IPacketPayloadNotification
    {
        [ProtoMember(1)]
        public Guid StreamId { get; set; }

        [ProtoMember(2)]
        public Guid TunnelPairId { get; set; }

        [ProtoMember(3)]
        public Guid EndpointPairId { get; set; }


        public NtPacketPayloadEndpointConnect(Guid tunnelPairId, Guid endpointPairId, Guid streamId)
        {
            StreamId = streamId;
            TunnelPairId = tunnelPairId;
            EndpointPairId = endpointPairId;
        }

        public NtPacketPayloadEndpointConnect()
        {
        }
    }
}
