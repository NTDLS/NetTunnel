using NetTunnel.Library.Types;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.PacketFraming.PacketPayloads.Queries;
using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;
using NetTunnel.Service.Types;
using System.Diagnostics;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening tunnel.
    /// </summary>
    internal class TunnelOutbound : BaseTunnel, ITunnel
    {
        private Thread? _outgoingConnectionThread;
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public TunnelOutbound(EngineCore core, NtTunnelOutboundConfiguration configuration)
            : base(core, configuration)
        {
            Address = configuration.Address;
            ManagementPort = configuration.ManagementPort;
            DataPort = configuration.DataPort;
            Username = configuration.Username;
            PasswordHash = configuration.PasswordHash;
        }

        public NtTunnelOutboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelOutboundConfiguration(PairId, Name, Address, ManagementPort, DataPort, Username, PasswordHash);

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

            Core.Logging.Write($"Starting outgoing tunnel '{Name}'");

            _outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            _outgoingConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            KeepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private void OutgoingConnectionThreadProc()
        {
            while (KeepRunning)
            {
                try
                {
                    Core.Logging.Write($"Attempting to connect to outgoing tunnel '{Name}' at {Address}:{DataPort}.");

                    var tcpClient = new TcpClient(Address, DataPort);

                    Core.Logging.Write($"Connection successful for tunnel '{Name}' at {Address}:{DataPort}.");

                    using (_stream = tcpClient.GetStream())
                    {
                        ExecuteStream(ProcessPacketNotificationCallback, ProcessPacketQueryCallback);
                    }

                    tcpClient.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                Thread.Sleep(1);
            }
        }

        private IPacketPayloadReply ProcessPacketQueryCallback(ITunnel tunnel, IPacketPayloadQuery packet)
        {
            if (packet is NtPacketPayloadAddEndpointInbound inboundEndpoint)
            {
                AddInboundEndpoint(inboundEndpoint.Configuration);
                Core.OutboundTunnels.SaveToDisk();
                return new NtPacketPayloadBoolean(true);
            }
            else if (packet is NtPacketPayloadAddEndpointOutbound outboundEndpoint)
            {
                AddOutboundEndpoint(outboundEndpoint.Configuration);
                Core.OutboundTunnels.SaveToDisk();
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
                var inboundEndpoint = _inboundEndpoints.Where(o=>o.PairId == exchange.EndpointPairId).FirstOrDefault();
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
