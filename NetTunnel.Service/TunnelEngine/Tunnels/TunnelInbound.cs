using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class TunnelInbound : BaseTunnel, ITunnel
    {
        private Thread? _inboundConnectionThread;
        public int DataPort { get; private set; }

        private readonly TcpListener _listener;

        public TunnelInbound(TunnelEngineCore core, NtTunnelInboundConfiguration configuration)
            : base(core, configuration)
        {
            DataPort = configuration.DataPort;

            _listener = new TcpListener(IPAddress.Any, DataPort);
        }

        public NtTunnelInboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelInboundConfiguration(PairId, Name, DataPort);

            foreach (var endpoint in _endpoints)
            {
                if (endpoint is EndpointInbound inboundEndpoint)
                {
                    var endpointConfiguration = new NtEndpointInboundConfiguration(inboundEndpoint.PairId, inboundEndpoint.Name, inboundEndpoint.Port);
                    tunnelConfiguration.EndpointInboundConfigurations.Add(endpointConfiguration);
                }
                else if (endpoint is EndpointOutbound outboundEndpoint)
                {
                    var endpointConfiguration = new NtEndpointOutboundConfiguration(outboundEndpoint.PairId, outboundEndpoint.Name, outboundEndpoint.Address, outboundEndpoint.Port);
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

            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Starting inbound tunnel '{Name}' on port {DataPort}.");
            base.Start();

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();

            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Starting endpoints for inbound tunnel '{Name}'.");
            _endpoints.ForEach(x => x.Start());
        }

        public override void Stop()
        {
            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopping inbound tunnel '{Name}' on port {DataPort}.");
            base.Stop();

            Utility.TryAndIgnore(_listener.Stop);
            Utility.TryAndIgnore(_listener.Stop);

            _inboundConnectionThread?.Join(); //Wait on thread to finish.
            Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopped inbound tunnel '{Name}'.");
        }

        private void InboundConnectionThreadProc()
        {
            try
            {
                _listener.Start();

                Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Started listiening for inbound tunnel '{Name}' on port {DataPort}.");

                while (KeepRunning)
                {
                    Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Waiting on connection for inbound tunnel '{Name}' on port {DataPort}.");

                    var tcpClient = _listener.AcceptTcpClient();
                    Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Accepted connection for inbound tunnel '{Name}' on port {DataPort}.");

                    using (Stream = tcpClient.GetStream())
                    {
                        ReceiveAndProcessStreamFrames(ProcessFrameNotificationCallback, ProcessFrameQueryCallback);
                        Utility.TryAndIgnore(Stream.Close);
                    }

                    Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Disconnected inbound tunnel '{Name}' on port {DataPort}");

                    Utility.TryAndIgnore(tcpClient.Close);
                    Utility.TryAndIgnore(tcpClient.Dispose);
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Write(Constants.NtLogSeverity.Exception, $"InboundConnectionThreadProc: {ex.Message}");
            }
            finally
            {
                Utility.TryAndIgnore(_listener.Stop);
            }
        }
    }
}
