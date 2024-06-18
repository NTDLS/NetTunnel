using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.ReliableMessageHandlers;
using NTDLS.ReliableMessaging;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    internal class TunnelOutbound : TunnelBase, ITunnel
    {
        private readonly NtServiceClient _client;
        private Thread? _establishConnectionThread;

        public TunnelOutbound(ServiceEngine core, NtTunnelConfiguration configuration)
            : base(core, configuration)
        {
            _client = NtServiceClient.Create(Singletons.Configuration,
                Configuration.Address, Configuration.ManagementPort, Configuration.Username, Configuration.PasswordHash, this);

            _client.Client.AddHandler(new OutboundTunnelNotificationHandlers());
            //_client.AddHandler(new oldTunnelOutboundQueryHandlers());

            _client.Client.OnConnected += _client_OnConnected;
            _client.Client.OnDisconnected += _client_OnDisconnected;

            _client.Client.OnException += (context, ex, payload) =>
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"RPC client exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }

        public override void NotificationEndpointExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
            => _client.NotificationEndpointExchange(tunnelId, endpointId, streamId, bytes, length);

        public override void NotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
            => _client.NotificationEndpointConnect(tunnelId, endpointId, streamId);

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

        public override void Start()
        {
            base.Start();

            _establishConnectionThread = new Thread(EstablishConnectionThread);
            _establishConnectionThread.Start();
        }

        public override void Stop()
        {
            _client.Disconnect();

            if (Environment.CurrentManagedThreadId != _establishConnectionThread?.ManagedThreadId)
            {
                _establishConnectionThread?.Join(); //Wait on thread to finish.
            }

            base.Stop();
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

                        _client.QueryRegisterTunnel(Configuration).Wait();

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


    }

}