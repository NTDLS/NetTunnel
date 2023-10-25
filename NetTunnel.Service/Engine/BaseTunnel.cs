using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.PacketFraming;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.PacketFraming.PacketPayloads.Queries;
using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;
using NetTunnel.Service.Types;
using System.Diagnostics;
using System.Net.Sockets;
using static NetTunnel.Service.PacketFraming.Types;

namespace NetTunnel.Service.Engine
{
    internal class BaseTunnel : ITunnel
    {
        private List<QueriesAwaitingReply> _queriesAwaitingReplies = new();

        protected NetworkStream? _stream { get; set; }

        public EngineCore Core { get; private set; }
        public bool KeepRunning { get; internal set; } = false;
        public Guid PairId { get; private set; }
        public string Name { get; private set; }

        internal readonly List<EndpointInbound> _inboundEndpoints = new();
        internal readonly List<EndpointOutbound> _outboundEndpoints = new();

        public BaseTunnel(EngineCore core, NtTunnelInboundConfiguration configuration)
        {
            Core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => _inboundEndpoints.Add(new(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => _outboundEndpoints.Add(new(Core, this, o)));
        }

        public BaseTunnel(EngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            Core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => _inboundEndpoints.Add(new(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => _outboundEndpoints.Add(new(Core, this, o)));
        }

        #region TCP/IP Packet and Stream interactions.

        /// <summary>
        /// Receive a message on the TCP/IP connection, parse and process the packets.
        /// </summary>
        internal void ReceiveAndProcessStreamPackets(ProcessPacketNotification processPacketNotificationCallback, ProcessPacketQuery processPacketQueryCallback)
        {
            try
            {
                var packetBuffer = new NtPacketBuffer();
                while (KeepRunning)
                {
                    NtPacketizer.ReceiveAndProcessStreamPackets(_stream, this, packetBuffer, processPacketNotificationCallback, processPacketQueryCallback);
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Write($"Exception in ReceiveAndProcessStreamPackets: {ex.Message}");
            }
        }

        /// <summary>
        /// We received a fire-and-forget message on the tunnel.
        /// </summary>
        /// <param name="tunnel"></param>
        /// <param name="packet"></param>
        /// <exception cref="Exception"></exception>
        internal void ProcessPacketNotificationCallback(ITunnel tunnel, IPacketPayloadNotification packet)
        {
            if (packet is NtPacketPayloadMessage message)
            {
                Debug.Print($"{message.Message}");
            }
            else if (packet is NtPacketPayloadEndpointConnect connectEndpoint)
            {
                _outboundEndpoints.Where(o => o.PairId == connectEndpoint.EndpointPairId).FirstOrDefault()?
                    .StartConnection(connectEndpoint.StreamId);
            }
            else if (packet is NtPacketPayloadEndpointDisconnect disconnectEndpoint)
            {
                GetEndpointById(disconnectEndpoint.EndpointPairId)?
                    .Disconnect(disconnectEndpoint.StreamId);
            }
            else if (packet is NtPacketPayloadEndpointExchange exchange)
            {
                GetEndpointById(exchange.EndpointPairId)?
                    .SendEndpointData(exchange.StreamId, exchange.Bytes);
            }
            else
            {
                throw new Exception("Unhandled notification packet.");
            }
        }

        /// <summary>
        /// We recevied a query on the tunnel. Reply to it.
        /// </summary>
        /// <param name="tunnel"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal IPacketPayloadReply ProcessPacketQueryCallback(ITunnel tunnel, IPacketPayloadQuery packet)
        {
            if (packet is NtPacketPayloadAddEndpointInbound inboundEndpoint)
            {
                AddInboundEndpoint(inboundEndpoint.Configuration);
                return new NtPacketPayloadBoolean(true);
            }
            else if (packet is NtPacketPayloadAddEndpointOutbound outboundEndpoint)
            {
                AddOutboundEndpoint(outboundEndpoint.Configuration);
                return new NtPacketPayloadBoolean(true);
            }
            else
            {
                throw new Exception("Unhandled query packet.");
            }
        }

        /// <summary>
        /// Sends a IPacketPayloadQuery that expects a IPacketPayloadReply in return.
        /// </summary>
        public async Task<T?> SendStreamPacketPayloadQuery<T>(IPacketPayloadQuery payload)
        {
            if (_stream == null)
            {
                throw new Exception("SendStreamPacketPayload stream can not be null.");
            }

            var cmd = new NtPacket()
            {
                EnclosedPayloadType = payload.GetType()?.FullName ?? string.Empty,
                Payload = Utility.SerializeToByteArray(payload)
            };

            var queriesAwaitingReply = new QueriesAwaitingReply()
            {
                PacketId = cmd.Id,
            };

            _queriesAwaitingReplies.Add(queriesAwaitingReply);

            return await Task.Run(() =>
            {
                var packetBytes = NtPacketizer.AssemblePacket(this, cmd);
                _stream.Write(packetBytes, 0, packetBytes.Length);

                //TODO: We need to check received data to see if any of them are replies
                if (queriesAwaitingReply.WaitEvent.WaitOne(NtPacketDefaults.QUERY_TIMEOUT_MS) == false)
                {
                    _queriesAwaitingReplies.Remove(queriesAwaitingReply);
                    throw new Exception("Query timeout expired while waiting on reply.");
                }

                _queriesAwaitingReplies.Remove(queriesAwaitingReply);

                return (T?)queriesAwaitingReply.ReplyPayload;
            });
        }

        /// <summary>
        /// Sends a reply to a IPacketPayloadQuery
        /// </summary>
        public void SendStreamPacketPayloadReply(NtPacket queryPacket, IPacketPayloadReply payload)
        {
            if (_stream == null)
            {
                throw new Exception("SendStreamPacketPayload stream can not be null.");
            }
            var cmd = new NtPacket()
            {
                Id = queryPacket.Id,
                EnclosedPayloadType = payload.GetType()?.FullName ?? string.Empty,
                Payload = Utility.SerializeToByteArray(payload)
            };

            var packetBytes = NtPacketizer.AssemblePacket(this, cmd);
            _stream.Write(packetBytes, 0, packetBytes.Length);
        }

        public void ApplyQueryReply(Guid packetId, IPacketPayloadReply replyPayload)
        {
            var waitingQuery = _queriesAwaitingReplies.Where(o => o.PacketId == packetId).Single();
            waitingQuery.ReplyPayload = replyPayload;
            waitingQuery.WaitEvent.Set();
        }

        /// <summary>
        /// Sends a one way (fire and forget) IPacketPayloadNotification.
        /// </summary>
        public void SendStreamPacketNotification(IPacketPayloadNotification payload)
        {
            if (_stream == null)
            {
                throw new Exception("SendStreamPacketPayload stream can not be null.");
            }
            var cmd = new NtPacket()
            {
                EnclosedPayloadType = payload.GetType()?.FullName ?? string.Empty,
                Payload = Utility.SerializeToByteArray(payload)
            };

            var packetBytes = NtPacketizer.AssemblePacket(this, cmd);
            _stream.Write(packetBytes, 0, packetBytes.Length);
        }

        #endregion

        public IEndpoint? GetEndpointById(Guid pairId)
        {
            var inboundEndpoint = _inboundEndpoints.Where(o => o.PairId == pairId).FirstOrDefault();
            if (inboundEndpoint != null)
            {
                return inboundEndpoint;
            }

            var outboundEndpoint = _outboundEndpoints.Where(o => o.PairId == pairId).FirstOrDefault();
            if (outboundEndpoint != null)
            {
                return outboundEndpoint;
            }

            return null;
        }

        #region Endpoint CRUD helpers.

        public void AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
        {
            _inboundEndpoints.Add(new EndpointInbound(Core, this, configuration));
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
        }

        public void AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
        {
            _outboundEndpoints.Add(new EndpointOutbound(Core, this, configuration));
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
        }

        public void DeleteInboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
        }

        public void DeleteOutboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
        }

        #endregion
    }
}
