using NetTunnel.Library.Types;
using NetTunnel.Service.Packetizer.PacketPayloads;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class TunnelInbound : BaseTunnel, ITunnel
    {
        private Thread? _incomingConnectionThread;
        public int DataPort { get; private set; }

        public TunnelInbound(EngineCore core, NtTunnelInboundConfiguration configuration)
            : base(core, configuration)
        {
            _core = core;

            DataPort = configuration.DataPort;

            configuration.InboundEndpointConfigurations.ForEach(o => _inboundEndpoints.Add(new(_core, this, o)));
            configuration.OutboundEndpointConfigurations.ForEach(o => _outboundEndpoints.Add(new(_core, this, o)));
        }

        public NtTunnelInboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelInboundConfiguration(PairId, Name, DataPort);

            foreach (var endpoint in _inboundEndpoints)
            {
                var endpointConfiguration = new NtEndpointInboundConfiguration(endpoint.PairId, endpoint.Name, endpoint.Port);
                tunnelConfiguration.InboundEndpointConfigurations.Add(endpointConfiguration);
            }

            foreach (var endpoint in _outboundEndpoints)
            {
                var endpointConfiguration = new NtEndpointOutboundConfiguration(endpoint.PairId, endpoint.Name, endpoint.Address, endpoint.Port);
                tunnelConfiguration.OutboundEndpointConfigurations.Add(endpointConfiguration);
            }

            return tunnelConfiguration;
        }

        public void Start()
        {
            KeepRunning = true;

            _core.Logging.Write($"Starting incoming tunnel '{Name}' on port {DataPort}");

            _incomingConnectionThread = new Thread(IncomingConnectionThreadProc);
            _incomingConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            KeepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private void IncomingConnectionThreadProc()
        {
            var listener = new TcpListener(IPAddress.Any, DataPort);

            try
            {
                listener.Start();

                _core.Logging.Write($"Listening incoming tunnel '{Name}' on port {DataPort}");

                while (KeepRunning)
                {
                    _core.Logging.Write($"Waiting for connection for incoming tunnel '{Name}' on port {DataPort}");

                    var tcpClient = listener.AcceptTcpClient();
                    _core.Logging.Write($"Connected on incoming tunnel '{Name}' on port {DataPort}");

                    using (_stream = tcpClient.GetStream())
                    {
                        ExecuteStream(ProcessPacketCallbackHandler);
                    }

                    tcpClient.Close();

                    _core.Logging.Write($"Disconnected incoming tunnel '{Name}' on port {DataPort}");

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Stop listening and close the listener when done.
                listener.Stop();
            }
        }

        void ProcessPacketCallbackHandler(ITunnel tunnel, IPacketPayload packet)
        {
            if (packet is NtPacketPayloadMessage)
            {
                var message = (NtPacketPayloadMessage)packet;
                Debug.Print($"{message.Message}");
            }
            else
            {
            }
        }
    }
}
