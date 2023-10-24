using NetTunnel.Library.Types;
using NetTunnel.Service.Packetizer;
using NetTunnel.Service.Packetizer.PacketPayloads.Notifications;
using NetTunnel.Service.Packetizer.PacketPayloads.Queries;
using NetTunnel.Service.Types;
using System.Net.Sockets;
using static NetTunnel.Service.Packetizer.NtPacketizer;

namespace NetTunnel.Service.Engine
{
    public class BaseTunnel : ITunnel
    {
        public bool KeepRunning { get; internal set; } = false;
        public Guid PairId { get; private set; }
        public string Name { get; private set; }

        internal NetworkStream? _stream { get; set; }
        internal EngineCore _core;
        internal readonly List<EndpointInbound> _inboundEndpoints = new();
        internal readonly List<EndpointOutbound> _outboundEndpoints = new();

        public BaseTunnel(EngineCore core, NtTunnelInboundConfiguration configuration)
        {
            _core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.InboundEndpointConfigurations.ForEach(o => _inboundEndpoints.Add(new(_core, this, o)));
            configuration.OutboundEndpointConfigurations.ForEach(o => _outboundEndpoints.Add(new(_core, this, o)));
        }

        public BaseTunnel(EngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            _core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.InboundEndpointConfigurations.ForEach(o => _inboundEndpoints.Add(new(_core, this, o)));
            configuration.OutboundEndpointConfigurations.ForEach(o => _outboundEndpoints.Add(new(_core, this, o)));
        }

        public void DispatchStreamPacketMessage(NtPacketPayloadMessage message) =>
            NtPacketizer.SendStreamPacketPayload(_stream, message);

        public void DispatchStreamPacketBytes(NtPacketPayloadBytes message) =>
            NtPacketizer.SendStreamPacketPayload(_stream, message);

        public async Task<T?> DispatchAddEndpointInbound<T>(NtEndpointInboundConfiguration configuration)
             => await NtPacketizer.SendStreamPacketPayloadQuery<T>(_stream, new NtPacketPayloadAddEndpointInbound(configuration));

        public async Task<T?> DispatchAddEndpointOutbound<T>(NtEndpointOutboundConfiguration configuration)
            => await NtPacketizer.SendStreamPacketPayloadQuery<T>(_stream, new NtPacketPayloadAddEndpointOutbound(configuration));

        internal void ExecuteStream(ProcessPacketNotification processPacketNotificationCallback, ProcessPacketQuery processPacketQueryCallback)
        {
            var packetBuffer = new NtPacketBuffer();

            while (KeepRunning)
            {
                DispatchStreamPacketMessage(new NtPacketPayloadMessage()
                {
                    Label = "This is the label.",
                    Message = "Message from...???."
                });

                NtPacketizer.ReceiveAndProcessStreamPackets(_stream, this, packetBuffer, processPacketNotificationCallback, processPacketQueryCallback);

                Thread.Sleep(1000);
            }
        }

        public void AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
                => _inboundEndpoints.Add(new EndpointInbound(_core, this, configuration));

        public void AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
            => _outboundEndpoints.Add(new EndpointOutbound(_core, this, configuration));

        public void DeleteInboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
        }

        public void DeleteOutboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
        }
    }
}
