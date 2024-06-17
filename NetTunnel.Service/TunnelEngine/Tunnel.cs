using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening tunnel service.
    /// </summary>
    internal class Tunnel
    {
        private readonly NtServiceClient _client;
        private Thread? _establishConnectionThread;

        public override int GetHashCode()
        {
            return TunnelId.GetHashCode()
                + Name.GetHashCode()
                + Endpoints.Sum(o => o.GetHashCode());
        }

        public int ChangeHash
            => TunnelId.GetHashCode()
            + Name.GetHashCode();

        #region Configuration Properties.

        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        #endregion

        #region Public Properties.

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
        private Thread? _heartbeatThread;

        #endregion

        public Tunnel(TunnelEngineCore core, NtTunnelConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
            Name = configuration.Name;

            configuration.EndpointConfigurations.Where(o => o.Direction == NtDirection.Inbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));

            configuration.EndpointConfigurations.Where(o => o.Direction == NtDirection.Outbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            Address = configuration.Address;
            ManagementPort = configuration.ManagementPort;
            Username = configuration.Username;
            PasswordHash = configuration.PasswordHash;

            _client = NtServiceClient.Create(Singletons.Configuration,
                configuration.Address, configuration.ManagementPort, configuration.Username, configuration.PasswordHash, this);

            //_client.AddHandler(new oldTunnelOutboundMessageHandlers());
            //_client.AddHandler(new oldTunnelOutboundQueryHandlers());

            _client.Client.OnConnected += _client_OnConnected;
            _client.Client.OnDisconnected += _client_OnDisconnected;

            _client.Client.OnException += (context, ex, payload) =>
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"RPC client exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }

        public IEndpoint? GetEndpointById(Guid pairId)
            => Endpoints.Where(o => o.EndpointId == pairId).SingleOrDefault();

        private void _client_OnDisconnected(RmContext context)
        {
            Status = NtTunnelStatus.Disconnected;

            Core.Logging.Write(NtLogSeverity.Warning, $"Tunnel '{Name}' disconnected.");
        }

        private void _client_OnConnected(RmContext context)
        {
            Status = NtTunnelStatus.Established;

            Core.Logging.Write(NtLogSeverity.Verbose, $"Tunnel '{Name}' connection successful.");
        }

        public NtTunnelConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelConfiguration(TunnelId, Name, Address, ManagementPort, Username, PasswordHash);

            foreach (var endpoint in Endpoints)
            {
                var endpointConfiguration = new NtEndpointConfiguration(TunnelId, endpoint.EndpointId,
                    (endpoint is EndpointInbound) ? NtDirection.Inbound : NtDirection.Outbound,
                    endpoint.Configuration.Name, endpoint.Configuration.OutboundAddress,
                    endpoint.Configuration.InboundPort, endpoint.Configuration.OutboundPort,
                    endpoint.Configuration.HttpHeaderRules, endpoint.Configuration.TrafficType);

                tunnelConfiguration.EndpointConfigurations.Add(endpointConfiguration);
            }

            return tunnelConfiguration;
        }

        public void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }
            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting tunnel '{Name}'.");

            KeepRunning = true;

            _establishConnectionThread = new Thread(EstablishConnectionThread);
            _establishConnectionThread.Start();

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();

            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting endpoints for tunnel '{Name}'.");
            Endpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopping tunnel '{Name}'.");
            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread?.Join();

            _client.Disconnect();

            if (Environment.CurrentManagedThreadId != _establishConnectionThread?.ManagedThreadId)
            {
                _establishConnectionThread?.Join(); //Wait on thread to finish.
            }

            Status = NtTunnelStatus.Stopped;

            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopped tunnel '{Name}'.");
        }

        /// <summary>
        /// This thread is used to make sure we stay connected to the tunnel service.
        /// </summary>
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

                        Core.Logging.Write(NtLogSeverity.Verbose, $"Tunnel '{Name}' connecting to service at {Address}:{ManagementPort}.");

                        //Make the outbound connection to the remote tunnel service.
                        _client.ConnectAndLogin().Wait();

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

        public EndpointInbound UpsertEndpoint(NtEndpointConfiguration configuration)
        {
            var existingEndpoint = GetEndpointById(configuration.EndpointId);
            if (existingEndpoint != null)
            {
                DeleteEndpoint(existingEndpoint.EndpointId);
            }

            var endpoint = new EndpointInbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            Core.Tunnels.SaveToDisk();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointPairId)
        {
            var endpoint = GetEndpointById(endpointPairId);
            if (endpoint != null)
            {
                endpoint.Stop();
                Endpoints.Remove(endpoint);
                Core.Tunnels.SaveToDisk();
            }
        }

        private void HeartbeatThreadProc()
        {
            Thread.CurrentThread.Name = $"HeartbeatThreadProc:{Environment.CurrentManagedThreadId}";

            DateTime lastHeartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastHeartBeat).TotalMilliseconds > Singletons.Configuration.TunnelAndEndpointHeartbeatDelayMs)
                {
                    var pingTime = _client.Ping();
                    Core.Logging.Write(NtLogSeverity.Verbose, $"Roundtrip time for '{Name}': {pingTime:n0}ms"); ;

                    lastHeartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }
    }
}
