using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.NASCCL;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal class BaseTunnel : ITunnel
    {
        public byte[]? EncryptionKey { get; protected set; }
        public bool SecureKeyExchangeIsComplete { get; protected set; }
        public NASCCLStream? NascclStream { get; protected set; }

        //public IRmEndpoint ReliableEndpoint { get; set; }

        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; internal set; }
        public ulong CurrentConnections { get; internal set; }
        public TunnelEngineCore Core { get; private set; }
        public bool KeepRunning { get; internal set; } = false;
        public Guid TunnelId { get; private set; }
        public string Name { get; private set; }

        public List<IEndpoint> Endpoints { get; set; } = new();

        private readonly Thread _heartbeatThread;

        public BaseTunnel(TunnelEngineCore core, NtTunnelInboundConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        public BaseTunnel(TunnelEngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
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
        public IEndpoint? GetEndpointById(Guid pairId) => Endpoints.Where(o => o.EndpointId == pairId).FirstOrDefault();

        #region TCP/IP frame and Stream interactions.

        /*
        
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
                    Core.InboundTunnels.Delete(deleteTunnel.TunnelId);
                    Core.InboundTunnels.SaveToDisk();
                }
                else
                {
                    Core.OutboundTunnels.Delete(deleteTunnel.TunnelId);
                    Core.OutboundTunnels.SaveToDisk();
                }
            }
            else if (frame is NtFramePayloadEndpointConnect connectEndpoint)
            {
                //Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Received endpoint connection notification.");

                Endpoints.OfType<EndpointOutbound>().Where(o => o.EndpointId == connectEndpoint.EndpointId).FirstOrDefault()?
                    .EstablishOutboundEndpointConnection(connectEndpoint.StreamId);
            }
            else if (frame is NtFramePayloadEndpointDisconnect disconnectEndpoint)
            {
                //Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Received endpoint disconnection notification.");

                GetEndpointById(disconnectEndpoint.EndpointId)?
                    .Disconnect(disconnectEndpoint.StreamId);
            }
            else if (frame is NtFramePayloadEndpointExchange exchange)
            {
                //Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Exchanging {exchange.Bytes.Length} bytes.");

                GetEndpointById(exchange.EndpointId)?
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
            else if (frame is NtFramePayloadDeleteEndpoint deleteEndpoint)
            {
                DeleteEndpoint(deleteEndpoint.EndpointId);
                return new NtFramePayloadBoolean(true);
            }
            else
            {
                throw new Exception($"ProcessFrameQueryCallback: Unhandled query frame type '{frame?.GetType()?.Name}'.");
            }
        }

        */

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
            var endpoint = Endpoints.Where(o => o.EndpointId == endpointPairId).SingleOrDefault();
            if (endpoint != null)
            {
                endpoint.Stop();
                Endpoints.Remove(endpoint);
                if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
                if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
            }
        }

        public virtual Task<T> Query<T>(IRmQuery<T> query) where T : class, IRmQueryReply
            => throw new Exception("Query<T>() must be overridden.");

        public virtual void Notify(IRmNotification notification)
            => throw new Exception("Notify() must be overridden.");

        #endregion
    }
}
