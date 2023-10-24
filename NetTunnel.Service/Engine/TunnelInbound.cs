using NetTunnel.Library.Types;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.PacketFraming.PacketPayloads.Queries;
using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;
using NetTunnel.Service.Types;
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
            DataPort = configuration.DataPort;
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
            if (KeepRunning == true)
            {
                return;
            }
            KeepRunning = true;

            Core.Logging.Write($"Starting incoming tunnel '{Name}' on port {DataPort}");

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

                Core.Logging.Write($"Listening incoming tunnel '{Name}' on port {DataPort}");

                while (KeepRunning)
                {
                    Core.Logging.Write($"Waiting for connection for incoming tunnel '{Name}' on port {DataPort}");

                    var tcpClient = listener.AcceptTcpClient();
                    Core.Logging.Write($"Connected on incoming tunnel '{Name}' on port {DataPort}");

                    using (_stream = tcpClient.GetStream())
                    {
                        ExecuteStream(ProcessPacketNotificationCallback, ProcessPacketQueryCallback);
                    }

                    tcpClient.Close();

                    Core.Logging.Write($"Disconnected incoming tunnel '{Name}' on port {DataPort}");

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

        private IPacketPayloadReply ProcessPacketQueryCallback(ITunnel tunnel, IPacketPayloadQuery packet)
        {
            if (packet is NtPacketPayloadAddEndpointInbound inboundEndpoint)
            {
                AddInboundEndpoint(inboundEndpoint.Configuration);
                Core.InboundTunnels.SaveToDisk();
                return new NtPacketPayloadBoolean(true);
            }
            else if (packet is NtPacketPayloadAddEndpointOutbound outboundEndpoint)
            {
                AddOutboundEndpoint(outboundEndpoint.Configuration);
                Core.InboundTunnels.SaveToDisk();
                return new NtPacketPayloadBoolean(true);
            }

            return new NtPacketPayloadBoolean(false);
        }

        private void ProcessPacketNotificationCallback(ITunnel tunnel, IPacketPayloadNotification packet)
        {
            if (packet is NtPacketPayloadMessage message)
            {
                Debug.Print($"{message.Message}");
            }
            else if (packet is NtPacketPayloadEndpointExchange exchange)
            {
                var inboundEndpoint = _inboundEndpoints.Where(o => o.PairId == exchange.EndpointPairId).FirstOrDefault();
                if (inboundEndpoint != null)
                {
                    inboundEndpoint.SendEndpointData(exchange.StreamId, exchange.Bytes);
                    return;
                }

                var outboundEndpoint = _outboundEndpoints.Where(o => o.PairId == exchange.EndpointPairId).FirstOrDefault();
                if (outboundEndpoint != null)
                {
                    outboundEndpoint.SendEndpointData(exchange.StreamId, exchange.Bytes);
                    return;
                }
            }
            else
            {
            }
        }
    }
}
