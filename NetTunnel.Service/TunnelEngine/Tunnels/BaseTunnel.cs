using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.MessageFraming;
using NetTunnel.Service.MessageFraming.FramePayloads.Notifications;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.MessageFraming.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.NASCCL;
using NTDLS.SecureKeyExchange;
using System.Diagnostics;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;
using static NetTunnel.Service.MessageFraming.Types;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal class BaseTunnel : ITunnel
    {
        private readonly List<QueriesAwaitingReply> _queriesAwaitingReplies = new();

        public byte[]? EncryptionKey { get; protected set; }
        public bool SecureKeyExchangeIsComplete { get; protected set; }
        public NASCCLStream? NascclStream { get; protected set; }

        protected NetworkStream? Stream { get; set; }

        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; internal set; }
        public ulong CurrentConnections { get; internal set; }
        public TunnelEngineCore Core { get; private set; }
        public bool KeepRunning { get; internal set; } = false;
        public Guid PairId { get; private set; }
        public string Name { get; private set; }

        public List<IEndpoint> Endpoints { get; set; } = new();

        private readonly Thread _heartbeatThread;

        public BaseTunnel(TunnelEngineCore core, NtTunnelInboundConfiguration configuration)
        {
            Core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        public BaseTunnel(TunnelEngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            Core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

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

            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread.Join();
        }

        private void HeartbeatThreadProc()
        {
            Thread.CurrentThread.Name = $"HeartbeatThreadProc:{Environment.CurrentManagedThreadId}";

            DateTime lastHeartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastHeartBeat).TotalMilliseconds > Singletons.Configuration.HeartbeatDelayMs)
                {
                    lastHeartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }
        public IEndpoint? GetEndpointById(Guid pairId) => Endpoints.Where(o => o.PairId == pairId).FirstOrDefault();

        #region TCP/IP frame and Stream interactions.

        /// <summary>
        /// Receive a message on the TCP/IP connection, parse and process the frames.
        /// </summary>
        internal void ReceiveAndProcessStreamFrames(ProcessFrameNotification processFrameNotificationCallback, ProcessFrameQuery processFrameQueryCallback)
        {
            try
            {
                var frameBuffer = new NtFrameBuffer(Singletons.Configuration.FramebufferSize);
                while (KeepRunning
                    && NtFraming.ReceiveAndProcessStreamFrames(Stream, this, frameBuffer, processFrameNotificationCallback, processFrameQueryCallback))
                {
                    //We have to set "SecureKeyExchangeIsComplete = true" after the message pump "ReceiveAndProcessStreamFrames" because otherwise the
                    //  last part of the key exchange is encrypted.
                    if (EncryptionKey != null && SecureKeyExchangeIsComplete == false)
                    {
                        NascclStream = new NASCCLStream(EncryptionKey);
                        SecureKeyExchangeIsComplete = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"ReceiveAndProcessStreamFrames: {ex.Message}");
            }
        }

        /// <summary>
        /// We received a fire-and-forget message on the tunnel.
        /// </summary>
        internal void ProcessFrameNotificationCallback(ITunnel tunnel, INtFramePayloadNotification frame)
        {
            if (EncryptionKey == null || SecureKeyExchangeIsComplete == false)
            {
                throw new Exception("Encryption has not been initialized.");
            }

            if (frame is NtFramePayloadMessage message)
            {
                Debug.Print($"{message.Message}");
            }
            else if (frame is NtFramePayloadDeleteTunnel deleteTunnel)
            {
                if (this is TunnelInbound)
                {
                    Core.InboundTunnels.Delete(deleteTunnel.TunnelPairId);
                    Core.InboundTunnels.SaveToDisk();
                }
                else
                {
                    Core.OutboundTunnels.Delete(deleteTunnel.TunnelPairId);
                    Core.OutboundTunnels.SaveToDisk();
                }
            }
            else if (frame is NtFramePayloadEndpointConnect connectEndpoint)
            {
                //Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Received endpoint connection notification.");

                Endpoints.OfType<EndpointOutbound>().Where(o => o.PairId == connectEndpoint.EndpointPairId).FirstOrDefault()?
                    .EstablishOutboundEndpointConnection(connectEndpoint.StreamId);
            }
            else if (frame is NtFramePayloadEndpointDisconnect disconnectEndpoint)
            {
                //Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Received endpoint disconnection notification.");

                GetEndpointById(disconnectEndpoint.EndpointPairId)?
                    .Disconnect(disconnectEndpoint.StreamId);
            }
            else if (frame is NtFramePayloadEndpointExchange exchange)
            {
                //Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Exchanging {exchange.Bytes.Length} bytes.");

                GetEndpointById(exchange.EndpointPairId)?
                    .SendEndpointData(exchange.StreamId, exchange.Bytes);
            }
            else
            {
                throw new Exception("ProcessFrameNotificationCallback: Unhandled notification frame type.");
            }
        }

        /// <summary>
        /// We recevied a query on the tunnel. Reply to it.
        /// </summary>
        internal INtFramePayloadReply ProcessFrameQueryCallback(ITunnel tunnel, INtFramePayloadQuery frame)
        {
            //We received a diffe-hellman key exhange request, respond to it so we can prop up encryption.
            if (frame is NtFramePayloadRequestKeyExchange keyExchangeRequest)
            {
                var compoundNegotiator = new CompoundNegotiator();
                byte[] negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(keyExchangeRequest.NegotiationToken);
                var negotiationReply = new NtFramePayloadKeyExchangeReply(negotiationReplyToken);
                EncryptionKey = compoundNegotiator.SharedSecret;
                return negotiationReply;
            }

            if (EncryptionKey == null || SecureKeyExchangeIsComplete == false)
            {
                throw new Exception("Encryption has not been initialized.");
            }

            if (frame is NtFramePayloadAddEndpointInbound inboundEndpoint)
            {
                var endpoint = AddInboundEndpoint(inboundEndpoint.Configuration);
                endpoint.Start();
                return new NtFramePayloadBoolean(true);
            }
            else if (frame is NtFramePayloadAddEndpointOutbound outboundEndpoint)
            {
                var endpoint = AddOutboundEndpoint(outboundEndpoint.Configuration);
                endpoint.Start();
                return new NtFramePayloadBoolean(true);
            }
            else
            {
                throw new Exception("ProcessFrameQueryCallback: Unhandled query frame type.");
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

            var queryAwaitingReply = new QueriesAwaitingReply()
            {
                FrameId = cmd.Id,
            };

            _queriesAwaitingReplies.Add(queryAwaitingReply);

            return await Task.Run(() =>
            {
                var frameBytes = NtFraming.AssembleFrame(this, cmd);
                Stream.Write(frameBytes, 0, frameBytes.Length);
                BytesSent += (ulong)frameBytes.Length;

                //Wait for a reply. When a reply is received, it will be routed to the correct query via ApplyQueryReply().
                //ApplyQueryReply() will apply the payload data to queryAwaitingReply and trigger the wait event.
                if (queryAwaitingReply.WaitEvent.WaitOne(Singletons.Configuration.FrameQueryTimeoutMs) == false)
                {
                    _queriesAwaitingReplies.Remove(queryAwaitingReply);
                    throw new Exception("Query timeout expired while waiting on reply.");
                }

                _queriesAwaitingReplies.Remove(queryAwaitingReply);

                return (T?)queryAwaitingReply.ReplyPayload;
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
            BytesSent += (ulong)frameBytes.Length;
        }

        /// <summary>
        /// A reply to a query was received, we need to find the waiting query - set the reply payload data and trigger the wait event.
        /// </summary>
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
            BytesSent += (ulong)frameBytes.Length;
        }

        #endregion

        #region Endpoint CRUD helpers.

        public EndpointInbound AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
        {
            var endpoint = new EndpointInbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
            return endpoint;
        }

        public EndpointOutbound AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
        {
            var endpoint = new EndpointOutbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointPairId)
        {
            var endpoint = Endpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            Endpoints.Remove(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
        }

        /*
        public void DeleteInboundEndpoint(Guid endpointPairId)
        {
            var endpoint = Endpoints.OfType<EndpointInbound>().Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            Endpoints.Remove(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
        }

        public void DeleteOutboundEndpoint(Guid endpointPairId)
        {
            var endpoint = Endpoints.OfType<EndpointOutbound>().Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            Endpoints.Remove(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
        }
        */

        #endregion
    }
}
