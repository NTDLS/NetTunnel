using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.Types;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class TunnelInbound : BaseTunnel, ITunnel
    {
        private Thread? _inboundConnectionThread;
        public int DataPort { get; private set; }

        private readonly TcpListener _listener;

        public TunnelInbound(EngineCore core, NtTunnelInboundConfiguration configuration)
            : base(core, configuration)
        {
            DataPort = configuration.DataPort;

            _listener = new TcpListener(IPAddress.Any, DataPort);
        }

        public NtTunnelInboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelInboundConfiguration(PairId, Name, DataPort);

            foreach (var endpoint in _inboundEndpoints)
            {
                var endpointConfiguration = new NtEndpointInboundConfiguration(endpoint.PairId, endpoint.Name, endpoint.Port);
                tunnelConfiguration.EndpointInboundConfigurations.Add(endpointConfiguration);
            }

            foreach (var endpoint in _outboundEndpoints)
            {
                var endpointConfiguration = new NtEndpointOutboundConfiguration(endpoint.PairId, endpoint.Name, endpoint.Address, endpoint.Port);
                tunnelConfiguration.EndpointOutboundConfigurations.Add(endpointConfiguration);
            }

            return tunnelConfiguration;
        }

        public override void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }

            Core.Logging.Write($"Starting inbound tunnel '{Name}' on port {DataPort}.");
            base.Start();

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();

            Core.Logging.Write($"Starting inbound endpoints for inbound tunnel '{Name}'.");
            _inboundEndpoints.ForEach(x => x.Start());

            Core.Logging.Write($"Starting outbound endpoints for inbound tunnel '{Name}'.");
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public override void Stop()
        {
            Core.Logging.Write($"Stopping inbound tunnel '{Name}' on port {DataPort}.");
            base.Stop();

            Utility.TryAndIgnore(_listener.Stop);
            Utility.TryAndIgnore(_listener.Stop);

            _inboundConnectionThread?.Join(); //Wait on thread to finish.
            Core.Logging.Write($"Stopped inbound tunnel '{Name}'.");
        }

        private void InboundConnectionThreadProc()
        {
            try
            {
                _listener.Start();

                Core.Logging.Write($"Started listiening for inbound tunnel '{Name}' on port {DataPort}.");

                while (KeepRunning)
                {
                    Core.Logging.Write($"Waiting on connection for inbound tunnel '{Name}' on port {DataPort}.");

                    var tcpClient = _listener.AcceptTcpClient();
                    Core.Logging.Write($"Accepted connection for inbound tunnel '{Name}' on port {DataPort}.");

                    using (Stream = tcpClient.GetStream())
                    {
                        ReceiveAndProcessStreamFrames(ProcessFrameNotificationCallback, ProcessFrameQueryCallback);
                        Utility.TryAndIgnore(Stream.Close);
                    }

                    Core.Logging.Write($"Disconnected inbound tunnel '{Name}' on port {DataPort}");

                    Utility.TryAndIgnore(tcpClient.Close);
                    Utility.TryAndIgnore(tcpClient.Dispose);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception[InboundConnectionThreadProc]: {ex.Message}");
            }
            finally
            {
                Utility.TryAndIgnore(_listener.Stop);
            }
        }
    }
}
