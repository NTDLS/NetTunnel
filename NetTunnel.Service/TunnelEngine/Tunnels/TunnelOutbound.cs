using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NetTunnel.Service.TunnelEngine.MessageHandlers;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening tunnel.
    /// </summary>
    internal class TunnelOutbound : BaseTunnel, ITunnel
    {
        private Thread? _outboundConnectionThread;
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

            _client.AddHandler(new TunnelOutboundMessageHandlers());

            _client.OnException += (RmContext context, Exception ex, IRmPayload? payload) =>
            {
                Core.Logging.Write(NtLogSeverity.Exception, $"RPC client exception: '{ex.Message}'"
                    + payload != null ? $"Payload: {payload?.GetType()?.Name}" : string.Empty);
            };
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

            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Starting outbound tunnel '{Name}'.");
            base.Start();

            _outboundConnectionThread = new Thread(OutboundConnectionThreadProc);
            _outboundConnectionThread.Start();

            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Starting endpoints for outbound tunnel '{Name}'.");
            Endpoints.ForEach(x => x.Start());
        }

        public override void Stop()
        {
            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopping outbound tunnel '{Name}'.");
            base.Stop();

            Utility.TryAndIgnore(_client.Disconnect);

            if (Environment.CurrentManagedThreadId != _outboundConnectionThread?.ManagedThreadId)
            {
                _outboundConnectionThread?.Join(); //Wait on thread to finish.
            }

            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopped outbound tunnel '{Name}'.");
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

        private void OutboundConnectionThreadProc()
        {
            Thread.CurrentThread.Name = $"OutboundConnectionThreadProc:{Environment.CurrentManagedThreadId}";

            while (KeepRunning)
            {
                try
                {
                    CurrentConnections++;
                    TotalConnections++;

                    Status = NtTunnelStatus.Connecting;

                    Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' connecting to remote at {Address}:{DataPort}.");

                    _client.Connect(Address, DataPort);

                    /*
                    Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' connection successful.");

                    Status = NtTunnelStatus.Established;

                    using (Stream = _tcpClient.GetStream())
                    {
                        ReceiveAndProcessStreamFrames(ProcessFrameNotificationCallback, ProcessFrameQueryCallback);
                    }

                    Status = NtTunnelStatus.Disconnected;

                    Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Outbound tunnel '{Name}' disconnected.");

                    _tcpClient.Close();
                    */
                }
                catch (Exception ex)
                {
                    Core.Logging.Write(Constants.NtLogSeverity.Exception, $"OutboundConnectionThreadProc: {ex.Message}");
                }
                finally
                {
                    CurrentConnections--;
                }
            }
        }
    }
}
