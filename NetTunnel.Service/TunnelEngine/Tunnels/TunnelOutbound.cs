using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessages;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;
using System.Net.Sockets;
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
        private CryptographyProvider? _cryptographyProvider;

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
                Parameter = this,
                //FrameDelimiter = Singletons.Configuration.FrameDelimiter,
                InitialReceiveBufferSize = Singletons.Configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = Singletons.Configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = Singletons.Configuration.ReceiveBufferGrowthRate,
            });

            _client.AddHandler(new TunnelOutboundMessageHandlers());
            _client.AddHandler(new TunnelOutboundQueryHandlers());

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

            _cryptographyProvider = null;
            SecureKeyExchangeIsComplete = false;
            //_client.ClearCryptographyProvider() //This can cause blocking.

            Core.Logging.Write(NtLogSeverity.Warning, $"Outbound tunnel '{Name}' disconnected.");
        }

        private void _client_OnConnected(RmContext context)
        {
            Status = NtTunnelStatus.Established;

            _cryptographyProvider = null;
            SecureKeyExchangeIsComplete = false;
            //_client.ClearCryptographyProvider() //This can cause blocking.

            Core.Logging.Write(NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' connection successful.");
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

        public void InitializeCryptographyProvider(byte[] sharedSecret)
        {
            _cryptographyProvider = new FramePayloads.CryptographyProvider(sharedSecret);

            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Encryption Key generated, hash: {Utility.ComputeSha256Hash(sharedSecret)}");
        }

        public void ApplyCryptographyProvider()
        {
            SecureKeyExchangeIsComplete = true;
            _client.SetCryptographyProvider(_cryptographyProvider);

            Core.Logging.Write(NtLogSeverity.Verbose, $"End-to-end encryption has been established for '{Name}'.");
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

                        _cryptographyProvider = null;
                        SecureKeyExchangeIsComplete = false;
                        _client.ClearCryptographyProvider();

                        Core.Logging.Write(NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' connecting to remote at {Address}:{DataPort}.");

                        //Make the outbound connection to the remote tunnel service.
                        _client.Connect(Address, DataPort);

                        //The first thing we do when we get a connection is start a new key exchange process.
                        var compoundNegotiator = new CompoundNegotiator();
                        var negotiationToken = compoundNegotiator.GenerateNegotiationToken(Singletons.Configuration.TunnelEncryptionKeySize);

                        var query = new QueryRequestKeyExchange(negotiationToken);
                        _client.Query(query, Singletons.Configuration.MessageQueryTimeoutMs).ContinueWith(t =>
                        {
                            if (t.IsCompletedSuccessfully && t.Result != null)
                            {
                                compoundNegotiator.ApplyNegotiationResponseToken(t.Result.NegotiationToken);

                                InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

                                _client.Notify(new NotificationApplyCryptography());

                                ApplyCryptographyProvider();
                            }
                        });

                        CurrentConnections++;
                        TotalConnections++;
                    }
                }
                catch (SocketException ex)
                {
                    Status = NtTunnelStatus.Disconnected;

                    if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        Core.Logging.Write(NtLogSeverity.Warning, $"EstablishConnectionThread: {ex.Message}");
                    }
                    else
                    {
                        Core.Logging.Write(NtLogSeverity.Exception, $"EstablishConnectionThread: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Status = NtTunnelStatus.Disconnected; //TODO: Are we really disconnected here??
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
