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
        private readonly List<QueriesAwaitingReply> _queriesAwaitingReplies = new();

        protected NetworkStream? Stream { get; set; }

        public EngineCore Core { get; private set; }
        public bool KeepRunning { get; internal set; } = false;
        public Guid PairId { get; private set; }
        public string Name { get; private set; }

        internal readonly List<EndpointInbound> _inboundEndpoints = new();
        internal readonly List<EndpointOutbound> _outboundEndpoints = new();

        private readonly Thread _heartbeatThread;

        public BaseTunnel(EngineCore core, NtTunnelInboundConfiguration configuration)
        {
            Core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => _inboundEndpoints.Add(new(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => _outboundEndpoints.Add(new(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        public BaseTunnel(EngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            Core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => _inboundEndpoints.Add(new(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => _outboundEndpoints.Add(new(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        public virtual void Start()
        {
            KeepRunning = true;
        }

        public virtual void Stop()
        {
            Utility.TryAndIgnore(() => Stream?.Close());

            _inboundEndpoints.ForEach(o => o.Stop());
            _outboundEndpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread.Join();
        }

        private void HeartbeatThreadProc()
        {
            DateTime lastheartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastheartBeat).TotalMilliseconds > 10000)
                {
                    lastheartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }

        #region TCP/IP frame and Stream interactions.

        /// <summary>
        /// Receive a message on the TCP/IP connection, parse and process the frames.
        /// </summary>
        internal void ReceiveAndProcessStreamFrames(ProcessFrameNotification processFrameNotificationCallback, ProcessFrameQuery processFrameQueryCallback)
        {
            try
            {
                var frameBuffer = new NtFrameBuffer();
                while (KeepRunning)
                {
                    NtFraming.ReceiveAndProcessStreamFrames(Stream, this, frameBuffer, processFrameNotificationCallback, processFrameQueryCallback);
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Write($"Exception in ReceiveAndProcessStreamFrames: {ex.Message}");
            }
        }

        /// <summary>
        /// We received a fire-and-forget message on the tunnel.
        /// </summary>
        /// <param name="tunnel"></param>
        /// <param name="frame"></param>
        /// <exception cref="Exception"></exception>
        internal void ProcessFrameNotificationCallback(ITunnel tunnel, INtFramePayloadNotification frame)
        {
            if (frame is NtFramePayloadMessage message)
            {
                Debug.Print($"{message.Message}");
            }
            else if (frame is NtFramePayloadEndpointConnect connectEndpoint)
            {
                _outboundEndpoints.Where(o => o.PairId == connectEndpoint.EndpointPairId).FirstOrDefault()?
                    .EstablishOutboundEndpointConnection(connectEndpoint.StreamId);
            }
            else if (frame is NtFramePayloadEndpointDisconnect disconnectEndpoint)
            {
                GetEndpointById(disconnectEndpoint.EndpointPairId)?
                    .Disconnect(disconnectEndpoint.StreamId);
            }
            else if (frame is NtFramePayloadEndpointExchange exchange)
            {
                GetEndpointById(exchange.EndpointPairId)?
                    .SendEndpointData(exchange.StreamId, exchange.Bytes);
            }
            else
            {
                throw new Exception("Unhandled notification frame.");
            }
        }

        /// <summary>
        /// We recevied a query on the tunnel. Reply to it.
        /// </summary>
        /// <param name="tunnel"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal INtFramePayloadReply ProcessFrameQueryCallback(ITunnel tunnel, INtFramePayloadQuery frame)
        {
            if (frame is NtFramePayloadAddEndpointInbound inboundEndpoint)
            {
                AddInboundEndpoint(inboundEndpoint.Configuration);
                return new NtFramePayloadBoolean(true);
            }
            else if (frame is NtFramePayloadAddEndpointOutbound outboundEndpoint)
            {
                AddOutboundEndpoint(outboundEndpoint.Configuration);
                return new NtFramePayloadBoolean(true);
            }
            else
            {
                throw new Exception("Unhandled query frame.");
            }
        }

        /// <summary>
        /// Sends a INtFramePayloadQuery that expects a INtFramePayloadReply in return.
        /// </summary>
        public async Task<T?> SendStreamFramePayloadQuery<T>(INtFramePayloadQuery payload)
        {
            if (Stream == null)
            {
                throw new Exception("SendStreamFramePayload stream can not be null.");
            }

            var cmd = new NtFrame()
            {
                EnclosedPayloadType = payload.GetType()?.FullName ?? string.Empty,
                Payload = Utility.SerializeToByteArray(payload)
            };

            var queriesAwaitingReply = new QueriesAwaitingReply()
            {
                FrameId = cmd.Id,
            };

            _queriesAwaitingReplies.Add(queriesAwaitingReply);

            return await Task.Run(() =>
            {
                var frameBytes = NtFraming.AssembleFrame(this, cmd);
                Stream.Write(frameBytes, 0, frameBytes.Length);

                //TODO: We need to check received data to see if any of them are replies
                if (queriesAwaitingReply.WaitEvent.WaitOne(NtFrameDefaults.QUERY_TIMEOUT_MS) == false)
                {
                    _queriesAwaitingReplies.Remove(queriesAwaitingReply);
                    throw new Exception("Query timeout expired while waiting on reply.");
                }

                _queriesAwaitingReplies.Remove(queriesAwaitingReply);

                return (T?)queriesAwaitingReply.ReplyPayload;
            });
        }

        /// <summary>
        /// Sends a reply to a INtFramePayloadQuery
        /// </summary>
        public void SendStreamFramePayloadReply(NtFrame queryFrame, INtFramePayloadReply payload)
        {
            if (Stream == null)
            {
                throw new Exception("SendStreamFramePayload stream can not be null.");
            }
            var cmd = new NtFrame()
            {
                Id = queryFrame.Id,
                EnclosedPayloadType = payload.GetType()?.FullName ?? string.Empty,
                Payload = Utility.SerializeToByteArray(payload)
            };

            var frameBytes = NtFraming.AssembleFrame(this, cmd);
            Stream.Write(frameBytes, 0, frameBytes.Length);
        }

        public void ApplyQueryReply(Guid frameId, INtFramePayloadReply replyPayload)
        {
            var waitingQuery = _queriesAwaitingReplies.Where(o => o.FrameId == frameId).Single();
            waitingQuery.ReplyPayload = replyPayload;
            waitingQuery.WaitEvent.Set();
        }

        /// <summary>
        /// Sends a one way (fire and forget) INtFramePayloadNotification.
        /// </summary>
        public void SendStreamFrameNotification(INtFramePayloadNotification payload)
        {
            if (Stream == null)
            {
                throw new Exception("SendStreamFramePayload stream can not be null.");
            }
            var cmd = new NtFrame()
            {
                EnclosedPayloadType = payload.GetType()?.FullName ?? string.Empty,
                Payload = Utility.SerializeToByteArray(payload)
            };

            var frameBytes = NtFraming.AssembleFrame(this, cmd);
            Stream.Write(frameBytes, 0, frameBytes.Length);
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
