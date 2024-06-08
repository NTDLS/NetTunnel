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
    /// This is the class that makes an outbound TCP/IP connection to a listening tunnel.
    /// </summary>
    internal class TunnelOutbound : ITunnel
    {
        private readonly RmClient _client;
        private Thread? _establishConnectionThread;

        #region Configuration Properties.

        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

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

        public TunnelOutbound(TunnelEngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();

            Address = configuration.Address;
            ManagementPort = configuration.ManagementPort;
            DataPort = configuration.DataPort;
            Username = configuration.Username;
            PasswordHash = configuration.PasswordHash;

            _client = new RmClient(new RmConfiguration()
            {
                //FrameDelimiter = Singletons.Configuration.FrameDelimiter,
                InitialReceiveBufferSize = Singletons.Configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = Singletons.Configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = Singletons.Configuration.ReceiveBufferGrowthRate,
            });

            _client.OnNotificationReceived += _client_OnNotificationReceived;
            _client.OnQueryReceived += _client_OnQueryReceived;
            _client.OnConnected += _client_OnConnected;
            _client.OnDisconnected += _client_OnDisconnected;

            _client.OnException += (RmContext? context, Exception ex, IRmPayload? payload) =>
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"RPC client exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }

        public IEndpoint? GetEndpointById(Guid pairId)
            => Endpoints.Where(o => o.EndpointId == pairId).FirstOrDefault();

        private void _client_OnDisconnected(RmContext context)
        {
            Status = NtTunnelStatus.Disconnected;

            _encryptionProvider = null;
            SecureKeyExchangeIsComplete = false;
            _client.ClearEncryptionProvider();

            Core.Logging.Write(NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' disconnected.");
        }

        private void _client_OnConnected(RmContext context)
        {
            Status = NtTunnelStatus.Established;

            _encryptionProvider = null;
            SecureKeyExchangeIsComplete = false;
            _client.ClearEncryptionProvider();

            Core.Logging.Write(NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' connection successful.");
        }

        private IRmQueryReply _client_OnQueryReceived(RmContext context, IRmPayload payload)
        {
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

        private void _client_OnNotificationReceived(RmContext context, IRmNotification payload)
        {
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
                Core.OutboundTunnels.Delete(deleteTunnel.TunnelId);
                Core.OutboundTunnels.SaveToDisk();
            }
            else if (payload is NtFramePayloadEndpointConnect connectEndpoint)
            {
                Core.Logging.Write(NtLogSeverity.Debug, $"Received endpoint connection notification.");

                Endpoints.OfType<EndpointOutbound>().Where(o => o.EndpointId == connectEndpoint.EndpointId).FirstOrDefault()?
                    .EstablishOutboundEndpointConnection(connectEndpoint.StreamId);
            }
            else if (payload is NtFramePayloadEndpointDisconnect disconnectEndpoint)
            {
                Core.Logging.Write(NtLogSeverity.Debug, $"Received endpoint disconnection notification.");

                GetEndpointById(disconnectEndpoint.EndpointId)?
                    .Disconnect(disconnectEndpoint.StreamId);
            }
            else if (payload is NtFramePayloadEndpointExchange exchange)
            {
                //Core.Logging.Write(NtLogSeverity.Debug, $"Exchanging {exchange.Bytes.Length:n0} bytes.");

                GetEndpointById(exchange.EndpointId)?
                    .SendEndpointData(exchange.StreamId, exchange.Bytes);
            }
        }

        public NtTunnelOutboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelOutboundConfiguration(TunnelId, Name, Address, ManagementPort, DataPort, Username, PasswordHash);

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
            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting outbound tunnel '{Name}'.");

            KeepRunning = true;

            _establishConnectionThread = new Thread(EstablishConnectionThread);
            _establishConnectionThread.Start();

            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting endpoints for outbound tunnel '{Name}'.");
            Endpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopping outbound tunnel '{Name}'.");
            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread.Join();

            Utility.TryAndIgnore(_client.Disconnect);

            if (Environment.CurrentManagedThreadId != _establishConnectionThread?.ManagedThreadId)
            {
                _establishConnectionThread?.Join(); //Wait on thread to finish.
            }

            Status = NtTunnelStatus.Stopped;

            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopped outbound tunnel '{Name}'.");
        }

        private void EstablishConnectionThread()
        {
            Thread.CurrentThread.Name = $"EstablishConnectionThread:{Environment.CurrentManagedThreadId}";

            while (KeepRunning)
            {
                try
                {
                    if (_client.IsConnected == false)
                    {
                        Status = NtTunnelStatus.Connecting;

                        _encryptionProvider = null;
                        SecureKeyExchangeIsComplete = false;
                        _client.ClearEncryptionProvider();

                        Core.Logging.Write(NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' connecting to remote at {Address}:{DataPort}.");

                        //Make the outbound connection to the remote tunnel service.
                        _client.Connect(Address, DataPort);

                        //The first thing we do when we get a connection is start a new key exchange process.
                        var compoundNegotiator = new CompoundNegotiator();
                        var negotiationToken = compoundNegotiator.GenerateNegotiationToken(Singletons.Configuration.TunnelEncryptionKeySize);

                        var query = new NtFramePayloadRequestKeyExchange(negotiationToken);
                        _client.Query(query, Singletons.Configuration.MessageQueryTimeoutMs).ContinueWith(t =>
                        {
                            if (t.IsCompletedSuccessfully && t.Result != null)
                            {
                                compoundNegotiator.ApplyNegotiationResponseToken(t.Result.NegotiationToken);
                                _encryptionProvider = new FramePayloads.EncryptionProvider(compoundNegotiator.SharedSecret);

                                Core.Logging.Write(NtLogSeverity.Verbose, $"Encryption Key generated, hash: {Utility.ComputeSha256Hash(compoundNegotiator.SharedSecret)}");

                                _client.Notify(new NtFramePayloadEncryptionReady());

                                SecureKeyExchangeIsComplete = true;
                                _client.SetEncryptionProvider(_encryptionProvider);

                                Core.Logging.Write(NtLogSeverity.Verbose, $"End-to-end encryption has been established for '{Name}'.");
                            }
                        });

                        CurrentConnections++;
                        TotalConnections++;
                    }
                }
                catch (Exception ex)
                {
                    Status = NtTunnelStatus.Disconnected;

                    Core.Logging.Write(NtLogSeverity.Exception, $"EstablishConnectionThread: {ex.Message}");
                }
                finally
                {
                    CurrentConnections--;
                }

                Thread.Sleep(1000);
            }
        }

        #region Reliable Messaging Passthrough.

        public Task<T> Query<T>(IRmQuery<T> query) where T : class, IRmQueryReply
        {
            if (_client.IsConnected == false)
            {
                throw new Exception("The RPC client is not connected.");
            }

            return _client.Query(query, Singletons.Configuration.MessageQueryTimeoutMs);
        }

        public void Notify(IRmNotification notification)
        {
            if (_client.IsConnected == false)
            {
                throw new Exception("The RPC client is not connected.");
            }

            _client.Notify(notification);
        }

        #endregion

        #region Endpoint CRUD helpers.

        public EndpointInbound AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
        {
            var endpoint = new EndpointInbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            Core.OutboundTunnels.SaveToDisk();
            return endpoint;
        }

        public EndpointOutbound AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
        {
            var endpoint = new EndpointOutbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            Core.OutboundTunnels.SaveToDisk();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointPairId)
        {
            var endpoint = Endpoints.Where(o => o.EndpointId == endpointPairId).SingleOrDefault();
            if (endpoint != null)
            {
                endpoint.Stop();
                Endpoints.Remove(endpoint);
                Core.OutboundTunnels.SaveToDisk();
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
