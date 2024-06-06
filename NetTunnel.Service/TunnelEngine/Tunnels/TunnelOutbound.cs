using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.FramePayloads.Notifications;
using NetTunnel.Service.FramePayloads.Queries;
using NetTunnel.Service.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening tunnel.
    /// </summary>
    internal class TunnelOutbound : BaseTunnel, ITunnel
    {
        private Thread? _establishConnectionThread;
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        private readonly RmClient _client;

        public TunnelOutbound(TunnelEngineCore core, NtTunnelOutboundConfiguration configuration)
            : base(core, configuration)
        {
            Address = configuration.Address;
            ManagementPort = configuration.ManagementPort;
            DataPort = configuration.DataPort;
            Username = configuration.Username;
            PasswordHash = configuration.PasswordHash;

            _client = new RmClient();

            _client.OnNotificationReceived += _client_OnNotificationReceived;
            _client.OnQueryReceived += _client_OnQueryReceived;
            _client.OnConnected += _client_OnConnected;
            _client.OnDisconnected += _client_OnDisconnected;

            _client.OnException += (RmContext context, Exception ex, IRmPayload? payload) =>
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"RPC client exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }

        private void _client_OnDisconnected(RmContext context)
        {
            Status = NtTunnelStatus.Disconnected;
            Core.Logging.Write(NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' disconnected.");
        }

        private void _client_OnConnected(RmContext context)
        {
            Status = NtTunnelStatus.Established;
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
            /*
            if (EncryptionKey == null || SecureKeyExchangeIsComplete == false)
            {
                throw new Exception("Encryption has not been initialized.");
            }
            */

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
                Core.Logging.Write(NtLogSeverity.Debug, $"Exchanging {exchange.Bytes.Length:n0} bytes.");

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

        public override void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }

            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting outbound tunnel '{Name}'.");
            base.Start();

            _establishConnectionThread = new Thread(EstablishConnectionThread);
            _establishConnectionThread.Start();

            Core.Logging.Write(NtLogSeverity.Verbose, $"Starting endpoints for outbound tunnel '{Name}'.");
            Endpoints.ForEach(x => x.Start());
        }

        public override void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopping outbound tunnel '{Name}'.");
            base.Stop();

            Utility.TryAndIgnore(_client.Disconnect);

            if (Environment.CurrentManagedThreadId != _establishConnectionThread?.ManagedThreadId)
            {
                _establishConnectionThread?.Join(); //Wait on thread to finish.
            }

            Core.Logging.Write(NtLogSeverity.Verbose, $"Stopped outbound tunnel '{Name}'.");
        }

        public override Task<T> Query<T>(IRmQuery<T> query)
        {
            if (_client.IsConnected == false)
            {
                throw new Exception("The RPC client is not connected.");
            }

            return _client.Query(query);
        }

        public override void Notify(IRmNotification notification)
        {
            if (_client.IsConnected == false)
            {
                throw new Exception("The RPC client is not connected.");
            }

            _client.Notify(notification);
        }

        private void EstablishConnectionThread()
        {
            Thread.CurrentThread.Name = $"EstablishConnectionThread:{Environment.CurrentManagedThreadId}";

            while (KeepRunning)
            {
                try
                {
                    if (Status != NtTunnelStatus.Established)
                    {
                        Status = NtTunnelStatus.Connecting;

                        Core.Logging.Write(NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' connecting to remote at {Address}:{DataPort}.");
                        _client.Connect(Address, DataPort);

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
            }
        }
    }
}
