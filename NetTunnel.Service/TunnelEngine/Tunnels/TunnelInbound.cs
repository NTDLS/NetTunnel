using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class TunnelInbound : ITunnel
    {
        public override int GetHashCode()
        {
            return TunnelId.GetHashCode()
                + Name.GetHashCode()
                + Endpoints.Sum(o => o.GetHashCode());
        }

        public int ChangeHash
            => GetHashCode();

        #region Configuration Properties.

        #endregion

        #region Properties Common to Inbound and Outbound Tunnels.

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

        #endregion

        public TunnelInbound(TunnelEngineCore core, NtTunnelInboundConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
            Name = configuration.Name;

            configuration.EndpointConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();

            /*
            _server = new RmServer(new RmConfiguration()
            {
                Parameter = this,
                //FrameDelimiter = Singletons.Configuration.FrameDelimiter,
                InitialReceiveBufferSize = Singletons.Configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = Singletons.Configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = Singletons.Configuration.ReceiveBufferGrowthRate,
            });

            _server.AddHandler(new TunnelInboundMessageHandlers());
            _server.AddHandler(new TunnelInboundQueryHandlers());

            _server.OnConnected += _server_OnConnected;
            _server.OnDisconnected += _server_OnDisconnected;

            _server.OnException += (RmContext? context, Exception ex, IRmPayload? payload) =>
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"RPC server exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
            */
        }

        public IEndpoint? GetEndpointById(Guid pairId)
            => Endpoints.Where(o => o.EndpointId == pairId).SingleOrDefault();

        public NtTunnelInboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelInboundConfiguration(TunnelId, Name);

            foreach (var endpoint in Endpoints)
            {
                if (endpoint is EndpointInbound ibe)
                {
                    var endpointConfiguration = new NtEndpointConfiguration(TunnelId,
                        ibe.EndpointId, NtDirection.Inbound, ibe.Configuration.Name, ibe.Configuration.OutboundAddress,
                        ibe.Configuration.InboundPort, ibe.Configuration.OutboundPort,
                        ibe.Configuration.HttpHeaderRules, ibe.Configuration.TrafficType);

                    tunnelConfiguration.EndpointConfigurations.Add(endpointConfiguration);
                }
                else if (endpoint is EndpointOutbound obe)
                {
                    var endpointConfiguration = new NtEndpointConfiguration(TunnelId,
                        obe.EndpointId, NtDirection.Outbound, obe.Configuration.Name, obe.Configuration.OutboundAddress,
                        obe.Configuration.InboundPort, obe.Configuration.OutboundPort,
                        obe.Configuration.HttpHeaderRules, obe.Configuration.TrafficType);

                    tunnelConfiguration.EndpointOutboundConfigurations.Add(endpointConfiguration);
                }
            }

            return tunnelConfiguration;
        }

        public void Start()
        {
            try
            {
                if (KeepRunning == true)
                {
                    return;
                }
                Core.Logging.Write(NtLogSeverity.Verbose, $"Starting inbound tunnel '{Name}'.");

                KeepRunning = true;

                Status = NtTunnelStatus.Disconnected;

                //_server.Start(DataPort);
                Core.Logging.Write(NtLogSeverity.Verbose, $"Started listening for inbound tunnel '{Name}'.");

                Core.Logging.Write(NtLogSeverity.Verbose, $"Starting endpoints for inbound tunnel '{Name}'.");
                Endpoints.ForEach(x => x.Start());
            }
            catch (Exception ex)
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"Failed to start tunnel '{Name}', exception: {ex.Message}.");

                Utility.TryAndIgnore(Stop);
            }
        }

        public void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopping inbound tunnel '{Name}'.");

            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread.Join();

            Status = NtTunnelStatus.Stopped;

            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopped inbound tunnel '{Name}'.");
        }

        #region Reliable Messaging Passthrough.

        /*
        public Task<T> Query<T>(IRmQuery<T> query) where T : class, IRmQueryReply
        {
            if (_peerRmClientConnectionId == null)
            {
                throw new Exception("The RPC server is not connected.");
            }

            var connectionId = _peerRmClientConnectionId.EnsureNotNullOrEmpty();

            if (_server.GetContext(connectionId) == null)
            {
                //Is there a reason we check this?
                throw new Exception("The RPC server context was not found.");
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

            if (_server.GetContext(connectionId) == null)
            {
                throw new Exception("The RPC server context was not found.");
            }

            _server.Notify(connectionId, notification);
        }
        */

        #endregion

        #region Endpoint CRUD helpers.

        /*
        public EndpointInbound UpsertInboundEndpoint(NtEndpointInboundConfiguration configuration)
        {
            var existingEndpoint = GetEndpointById(configuration.EndpointId);
            if (existingEndpoint != null)
            {
                DeleteEndpoint(existingEndpoint.EndpointId);
            }

            var endpoint = new EndpointInbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            Core.InboundTunnels.SaveToDisk();
            return endpoint;
        }

        public EndpointOutbound UpsertOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
        {
            var existingEndpoint = GetEndpointById(configuration.EndpointId);
            if (existingEndpoint != null)
            {
                DeleteEndpoint(existingEndpoint.EndpointId);
            }

            var endpoint = new EndpointOutbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            Core.InboundTunnels.SaveToDisk();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointPairId)
        {
            var endpoint = GetEndpointById(endpointPairId);
            if (endpoint != null)
            {
                endpoint.Stop();
                Endpoints.Remove(endpoint);
                Core.InboundTunnels.SaveToDisk();
            }
        }
        */

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
