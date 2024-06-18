﻿using NetTunnel.Library;
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
            return Configuration.TunnelId.GetHashCode()
                + Configuration.Name.GetHashCode()
                + Endpoints.Sum(o => o.GetHashCode());
        }

        public int ChangeHash
            => Configuration.TunnelId.GetHashCode()
            + Configuration.Name.GetHashCode();

        #region Public Properties.

        public NtTunnelConfiguration Configuration { get; private set; }

        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; private set; }
        public ulong CurrentConnections { get; private set; }
        public ServiceEngine Core { get; private set; }
        public bool KeepRunning { get; private set; } = false;
        //public Guid TunnelId { get; private set; }
        //public string Name { get; private set; }
        public List<IEndpoint> Endpoints { get; private set; } = new();
        private Thread? _heartbeatThread;

        #endregion

        public Tunnel(ServiceEngine core, NtTunnelConfiguration configuration)
        {
            Core = core;
            Configuration = configuration.CloneConfiguration();

            Configuration.Endpoints.Where(o => o.Direction == NtDirection.Inbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));

            Configuration.Endpoints.Where(o => o.Direction == NtDirection.Outbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));


            _client = NtServiceClient.Create(Singletons.Configuration,
                Configuration.Address, Configuration.ManagementPort, Configuration.Username, Configuration.PasswordHash, this);

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

        public void NotificationEndpointExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
            => _client.NotificationEndpointExchange(tunnelId, endpointId, streamId, bytes, length);

        public void NotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
            => _client.NotificationEndpointConnect(tunnelId, endpointId, streamId);

        public IEndpoint? GetEndpointById(Guid pairId)
            => Endpoints.Where(o => o.EndpointId == pairId).SingleOrDefault();

        private void _client_OnDisconnected(RmContext context)
        {
            Status = NtTunnelStatus.Disconnected;

            Core.Logging.Write(NtLogSeverity.Warning, $"Tunnel '{Configuration.Name}' disconnected.");
        }

        private void _client_OnConnected(RmContext context)
        {
            Status = NtTunnelStatus.Established;

            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Tunnel '{Configuration.Name}' connection successful.");
        }

        public NtTunnelConfiguration CloneConfiguration()
        {
            return Configuration.CloneConfiguration();
        }

        public void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }
            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Starting tunnel '{Configuration.Name}'.");

            KeepRunning = true;

            _establishConnectionThread = new Thread(EstablishConnectionThread);
            _establishConnectionThread.Start();

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();

            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Starting endpoints for tunnel '{Configuration.Name}'.");

            Endpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Stopping tunnel '{Configuration.Name}'.");

            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread?.Join();

            _client.Disconnect();

            if (Environment.CurrentManagedThreadId != _establishConnectionThread?.ManagedThreadId)
            {
                _establishConnectionThread?.Join(); //Wait on thread to finish.
            }

            Status = NtTunnelStatus.Stopped;

            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Stopped tunnel '{Configuration.Name}'.");
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

                        Core.Logging.Write(NtLogSeverity.Verbose,
                            $"Tunnel '{Configuration.Name}' connecting to service at {Configuration.Address}:{Configuration.ManagementPort}.");

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
                        Core.Logging.Write(NtLogSeverity.Warning,
                            $"EstablishConnectionThread: {ex.Message}");
                    }
                    else
                    {
                        Core.Logging.Write(NtLogSeverity.Exception,
                            $"EstablishConnectionThread: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Status = NtTunnelStatus.Disconnected; //TODO: Are we really disconnected here??

                    Core.Logging.Write(NtLogSeverity.Exception,
                        $"EstablishConnectionThread: {ex.Message}");
                }
                finally
                {
                    CurrentConnections--;
                }

                Thread.Sleep(1000);
            }
        }

        #region Add/Delete Endpoints.

        public EndpointInbound UpsertEndpoint(NtEndpointConfiguration configuration)
        {
            var existingEndpoint = GetEndpointById(configuration.EndpointId);
            if (existingEndpoint != null)
            {
                DeleteEndpoint(existingEndpoint.EndpointId);
            }

            var endpoint = new EndpointInbound(Core, this, configuration);
            Configuration.Endpoints.Add(configuration);
            Endpoints.Add(endpoint);
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointId)
        {
            var endpoint = GetEndpointById(endpointId);
            if (endpoint != null)
            {
                endpoint.Stop();
                Configuration.Endpoints.RemoveAll(o => o.EndpointId == endpointId);
                Endpoints.Remove(endpoint);
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
                    var pingTime = _client.Ping();
                    Core.Logging.Write(NtLogSeverity.Debug,
                        $"Roundtrip time for '{Configuration.Name}': {pingTime:n0}ms"); ;

                    lastHeartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }
    }
}
