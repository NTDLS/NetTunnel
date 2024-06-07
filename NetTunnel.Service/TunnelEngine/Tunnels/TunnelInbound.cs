using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.FramePayloads.Notifications;
using NetTunnel.Service.FramePayloads.Queries;
using NetTunnel.Service.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class TunnelInbound : ITunnel
    {
        private readonly RmServer _server;
        private Guid? _peerRmClientConnectionId;

        #region Configuration Properties.

        public int DataPort { get; private set; }

        #endregion

        #region Properties Common to Inbound and Outbound Tunnels.

        public bool SecureKeyExchangeIsComplete { get; private set; }
        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; private set; }
        public ulong CurrentConnections { get; private set; }
        public TunnelEngineCore Core { get; private set; }
        public bool KeepRunning { get; private set; } = false;
        public Guid TunnelId { get; private set; }
        public string Name { get; private set; }
        public List<IEndpoint> Endpoints { get; private set; } = new();

        private readonly Thread _heartbeatThread;
        private FramePayloads.EncryptionProvider? _encryptionProvider;

        #endregion

        public TunnelInbound(TunnelEngineCore core, NtTunnelInboundConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();

            DataPort = configuration.DataPort;

            _server = new RmServer(new RmConfiguration()
            {
                FrameDelimiter = Singletons.Configuration.FrameDelimiter,
                InitialReceiveBufferSize = Singletons.Configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = Singletons.Configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = Singletons.Configuration.ReceiveBufferGrowthRate,
            });

            _server.OnNotificationReceived += _server_OnNotificationReceived;
            _server.OnQueryReceived += _server_OnQueryReceived;
            _server.OnConnected += _server_OnConnected;
            _server.OnDisconnected += _server_OnDisconnected;

            _server.OnException += (RmContext context, Exception ex, IRmPayload? payload) =>
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"RPC server exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }

        public IEndpoint? GetEndpointById(Guid pairId)
            => Endpoints.Where(o => o.EndpointId == pairId).FirstOrDefault();

        private void _server_OnConnected(RmContext context)
        {
            if (_peerRmClientConnectionId != null)
            {
                Core.Logging.Write(NtLogSeverity.Verbose, $"The tunnel '{Name}' on port {DataPort} cannot accept more than one connection.");
                context.Disconnect();
                return;
            }

            Core.Logging.Write(NtLogSeverity.Verbose, $"Accepted connection for inbound tunnel '{Name}' on port {DataPort}.");
            Status = NtTunnelStatus.Established;

            TotalConnections++;
            CurrentConnections++;

            _peerRmClientConnectionId = context.ConnectionId;
        }

        private void _server_OnDisconnected(RmContext context)
        {
            Status = NtTunnelStatus.Disconnected;

            CurrentConnections--;

            Core.Logging.Write(NtLogSeverity.Verbose, $"Disconnected inbound tunnel '{Name}' on port {DataPort}");

            _peerRmClientConnectionId = null;
        }

        private IRmQueryReply _server_OnQueryReceived(RmContext context, IRmPayload payload)
        {
            //We received a diffie–hellman key exchange request, respond to it so we can prop up encryption.
            if (payload is NtFramePayloadRequestKeyExchange keyExchangeRequest)
            {
                var compoundNegotiator = new CompoundNegotiator();
                var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(keyExchangeRequest.NegotiationToken);
                var negotiationReply = new NtFramePayloadKeyExchangeReply(negotiationReplyToken);
                _encryptionProvider = new FramePayloads.EncryptionProvider(compoundNegotiator.SharedSecret);
                return negotiationReply;
            }

            if (SecureKeyExchangeIsComplete == false)
            {
                throw new Exception("Encryption has not been initialized.");
            }

            if (payload is NtFramePayloadAddEndpointInbound inboundEndpoint)
            {
                var endpoint = AddInboundEndpoint(inboundEndpoint.Configuration);
                endpoint.Start();
                return new NtFramePayloadBoolean(true);
            }
            else if (payload is NtFramePayloadAddEndpointOutbound outboundEndpoint)
            {
                var endpoint = AddOutboundEndpoint(outboundEndpoint.Configuration);
                endpoint.Start();
                return new NtFramePayloadBoolean(true);
            }
            else if (payload is NtFramePayloadDeleteEndpoint deleteEndpoint)
            {
                DeleteEndpoint(deleteEndpoint.EndpointId);
                return new NtFramePayloadBoolean(true);
            }

            throw new Exception($"RPC query handler not defined for: {payload?.GetType()?.Name}");
        }

        private void _server_OnNotificationReceived(RmContext context, IRmNotification payload)
        {
            if (payload is NtFramePayloadEncryptionReady)
            {
                Core.Logging.Write(NtLogSeverity.Debug, $"Encryption is ready.'");

                SecureKeyExchangeIsComplete = true;
                _server.SetEncryptionProvider(_encryptionProvider);
            }

            if (SecureKeyExchangeIsComplete == false)
            {
                throw new Exception("Encryption has not been initialized.");
            }

            if (payload is NtFramePayloadMessage message)
            {
                Core.Logging.Write(NtLogSeverity.Debug, $"RPC Message: '{message.Message}'");
            }
            else if (payload is NtFramePayloadDeleteTunnel deleteTunnel)
            {
                Core.InboundTunnels.Delete(deleteTunnel.TunnelId);
                Core.InboundTunnels.SaveToDisk();
            }
            else if (payload is NtFramePayloadEndpointConnect connectEndpoint)
            {
                Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Received endpoint connection notification.");

                Endpoints.OfType<EndpointOutbound>().Where(o => o.EndpointId == connectEndpoint.EndpointId).FirstOrDefault()?
                    .EstablishOutboundEndpointConnection(connectEndpoint.StreamId);
            }
            else if (payload is NtFramePayloadEndpointDisconnect disconnectEndpoint)
            {
                Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Received endpoint disconnection notification.");

                GetEndpointById(disconnectEndpoint.EndpointId)?
                    .Disconnect(disconnectEndpoint.StreamId);
            }
            else if (payload is NtFramePayloadEndpointExchange exchange)
            {
                //Core.Logging.Write(Constants.NtLogSeverity.Debug, $"Exchanging {exchange.Bytes.Length:n0} bytes.");

                GetEndpointById(exchange.EndpointId)?
                    .SendEndpointData(exchange.StreamId, exchange.Bytes);
            }
        }

        public NtTunnelInboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelInboundConfiguration(TunnelId, Name, DataPort);

            foreach (var endpoint in Endpoints)
            {
                if (endpoint is EndpointInbound inboundEndpoint)
                {
                    var endpointConfiguration = new NtEndpointInboundConfiguration(TunnelId, inboundEndpoint.EndpointId, inboundEndpoint.Name, inboundEndpoint.TransmissionPort);
                    tunnelConfiguration.EndpointInboundConfigurations.Add(endpointConfiguration);
                }
                else if (endpoint is EndpointOutbound outboundEndpoint)
                {
                    var endpointConfiguration = new NtEndpointOutboundConfiguration(TunnelId, outboundEndpoint.EndpointId, outboundEndpoint.Name, outboundEndpoint.Address, outboundEndpoint.TransmissionPort);
                    tunnelConfiguration.EndpointOutboundConfigurations.Add(endpointConfiguration);
                }
            }

            return tunnelConfiguration;
        }

        public void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }
            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting inbound tunnel '{Name}' on port {DataPort}.");

            KeepRunning = true;

            Status = NtTunnelStatus.Disconnected;

            _server.Start(DataPort);
            Core.Logging.Write(NtLogSeverity.Verbose, $"Started listening for inbound tunnel '{Name}' on port {DataPort}.");

            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting endpoints for inbound tunnel '{Name}'.");
            Endpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopping inbound tunnel '{Name}' on port {DataPort}.");

            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread.Join();
            Utility.TryAndIgnore(_server.Stop);

            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopped inbound tunnel '{Name}'.");
        }

        #region Reliable Messaging Passthrough.

        public Task<T> Query<T>(IRmQuery<T> query) where T : class, IRmQueryReply
        {
            if (_peerRmClientConnectionId == null)
            {
                throw new Exception("The RPC server is not connected.");
            }

            var connectionId = _peerRmClientConnectionId.EnsureNotNullOrEmpty();

            if (_server.GetClient(connectionId) == null)
            {
                throw new Exception("The RPC server client was not found.");
            }

            return _server.Query(connectionId, query, Singletons.Configuration.MessageQueryTimeoutMs);
        }

        public void Notify(IRmNotification notification)
        {
            if (_peerRmClientConnectionId == null)
            {
                throw new Exception("The RPC server is not connected.");
            }

            var connectionId = _peerRmClientConnectionId.EnsureNotNullOrEmpty();

            if (_server.GetClient(connectionId) == null)
            {
                throw new Exception("The RPC server client was not found.");
            }

            _server.Notify(connectionId, notification);
        }

        #endregion

        #region Endpoint CRUD helpers.

        public EndpointInbound AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
        {
            var endpoint = new EndpointInbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            Core.InboundTunnels.SaveToDisk();
            return endpoint;
        }

        public EndpointOutbound AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
        {
            var endpoint = new EndpointOutbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            Core.InboundTunnels.SaveToDisk();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointPairId)
        {
            var endpoint = Endpoints.Where(o => o.EndpointId == endpointPairId).SingleOrDefault();
            if (endpoint != null)
            {
                endpoint.Stop();
                Endpoints.Remove(endpoint);
                Core.InboundTunnels.SaveToDisk();
            }
        }

        #endregion

        private void HeartbeatThreadProc()
        {
            Thread.CurrentThread.Name = $"HeartbeatThreadProc:{Environment.CurrentManagedThreadId}";

            DateTime lastHeartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastHeartBeat).TotalMilliseconds > Singletons.Configuration.TunnelAndEndpointHeartbeatDelayMs)
                {
                    lastHeartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }

    }
}
