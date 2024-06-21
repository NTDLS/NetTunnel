﻿using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.ReliableHandlers;
using NTDLS.ReliableMessaging;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    internal class TunnelOutbound : TunnelBase, ITunnel
    {
        private readonly ServiceClient _client;
        private Thread? _establishConnectionThread;

        public override NtDirection Direction { get => NtDirection.Outbound; }

        public bool IsLoggedIn => _client.IsLoggedIn;

        public TunnelOutbound(ServiceEngine serviceEngine, TunnelConfiguration configuration)
            : base(serviceEngine, configuration)
        {
            _client = ServiceClient.Create(serviceEngine.Logger, Singletons.Configuration,
                Configuration.Address, Configuration.ManagementPort, Configuration.Username, Configuration.PasswordHash, this);

            _client.Client.AddHandler(new TunnelOutboundNotificationHandlers());
            //_client.AddHandler(new oldTunnelOutboundQueryHandlers());

            _client.Client.OnConnected += Client_OnConnected;
            _client.Client.OnDisconnected += Client_OnDisconnected;

            _client.Client.OnException += (context, ex, payload) =>
            {
                ServiceEngine.Logger.Exception($"RPC client exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }

        #region Interface: ITunnel.

        /// <summary>
        /// Sends a notification to the remote tunnel service containing the data that was received
        ///     by an endpoint. This data is to be sent to the endpoint connection with the matching
        ///     edgeId (which was originally sent to PeerNotifyOfEndpointConnect()
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        /// <param name="bytes">Bytes to be sent to endpoint connection.</param>
        /// <param name="length">Number of bytes to be sent to the endpoint connection.</param>
        public void PeerNotifyOfEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, byte[] bytes, int length)
            => _client.NotificationEndpointExchange(tunnelKey, endpointId, edgeId, bytes, length);

        /// <summary>
        /// Sends a notification to the remote tunnel service to let it know to connect
        ///     the associated outbound endpoint for an incoming endpoint connection.
        /// </summary>
        /// <param name="tunnelKey">The id of the tunnel that owns the endpoint.</param>
        /// <param name="endpointId">The id of the endpoint that owns the connection.</param>
        /// <param name="edgeId">The id that will uniquely identity the associated endpoint connections at each service</param>
        public void PeerNotifyOfEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => _client.NotificationEndpointConnect(tunnelKey, endpointId, edgeId);

        public void PeerNotifyOfEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId)
            => _client.PeerNotifyOfEndpointDisconnect(tunnelKey, endpointId, edgeId);

        public void PeerNotifyOfTunnelDeletion(DirectionalKey tunnelKey)
            => _client.PeerNotifyOfTunnelDeletion(tunnelKey);

        public void PeerNotifyOfEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId)
            => _client.PeerNotifyOfEndpointDeletion(tunnelKey, endpointId);

        #endregion

        private void Client_OnDisconnected(RmContext context)
        {
            Status = NtTunnelStatus.Disconnected;

            CurrentConnections--;

            ServiceEngine.Logger.Warning($"Tunnel '{Configuration.Name}' disconnected.");
        }

        private void Client_OnConnected(RmContext context)
        {
            Status = NtTunnelStatus.Established;

            CurrentConnections++;
            TotalConnections++;

            ServiceEngine.Logger.Verbose($"Tunnel '{Configuration.Name}' connection successful.");
        }

        public void EnforceLogin()
        {
            if (_client.IsLoggedIn != true)
            {
                throw new Exception("Client is not logged in or encryption has not been established.");
            }
        }

        public override void Start()
        {
            base.Start();

            _establishConnectionThread = new Thread(EstablishConnectionThread);
            _establishConnectionThread.Start();
        }

        public override void Stop()
        {
            base.Stop();

            _client.Disconnect();

            if (Environment.CurrentManagedThreadId != _establishConnectionThread?.ManagedThreadId)
            {
                _establishConnectionThread?.Join(); //Wait on thread to finish.
            }

            Status = NtTunnelStatus.Stopped;
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

                        ServiceEngine.Logger.Verbose(
                            $"Tunnel '{Configuration.Name}' connecting to service at {Configuration.Address}:{Configuration.ManagementPort}.");

                        //Make the outbound connection to the remote tunnel service.
                        _client.ConnectAndLogin();

                        _client.QueryRegisterTunnel(Configuration);

                        Status = NtTunnelStatus.Established;
                    }
                }
                catch (Exception ex)
                {
                    Status = NtTunnelStatus.Disconnected;

                    if (ex.InnerException is SocketException sockEx)
                    {
                        if (sockEx.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            ServiceEngine.Logger.Warning(
                                $"EstablishConnectionThread: {ex.Message}");
                        }
                        else
                        {
                            ServiceEngine.Logger.Exception(ex, $"EstablishConnectionThread: {ex.Message}");
                        }
                    }
                    else
                    {
                        ServiceEngine.Logger.Exception(ex, $"EstablishConnectionThread: {ex.Message}");
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}