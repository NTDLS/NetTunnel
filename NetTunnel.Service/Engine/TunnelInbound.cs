using NetTunnel.ClientAPI;
using NetTunnel.Library.Types;
using NetTunnel.Service.Packetizer;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    public class TunnelInbound : ITunnel
    {
        private readonly EngineCore _core;
        private Thread? _incomingConnectionThread;
        private bool _keepRunning = false;
        private NetworkStream? _stream;

        private readonly List<EndpointInbound> _inboundEndpoints = new();
        private readonly List<EndpointOutbound> _outboundEndpoints = new();

        public Guid PairId { get; private set; }
        public string Name { get; private set; }
        public int DataPort { get; private set; }

        public TunnelInbound(EngineCore core, NtTunnelInboundConfiguration configuration)
        {
            _core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;
            DataPort = configuration.DataPort;

            configuration.InboundEndpointConfigurations.ForEach(o => _inboundEndpoints.Add(new(_core, this, o)));
            configuration.OutboundEndpointConfigurations.ForEach(o => _outboundEndpoints.Add(new(_core, this, o)));
        }

        public void DispatchMessage(string message)
        {
            throw new NotImplementedException();
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

        public void AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
            => _inboundEndpoints.Add(new EndpointInbound(_core, this, configuration));

        public void AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
            => _outboundEndpoints.Add(new EndpointOutbound(_core, this, configuration));

        public void DeleteInboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
        }

        public void DeleteOutboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
        }

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting incoming tunnel '{Name}' on port {DataPort}");

            _incomingConnectionThread = new Thread(IncomingConnectionThreadProc);
            _incomingConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private void IncomingConnectionThreadProc()
        {
            var listener = new TcpListener(IPAddress.Any, DataPort);

            try
            {
                listener.Start();

                _core.Logging.Write($"Listening incoming tunnel '{Name}' on port {DataPort}");

                while (_keepRunning)
                {
                    _core.Logging.Write($"Waiting for connection for incoming tunnel '{Name}' on port {DataPort}");

                    var client = listener.AcceptTcpClient();
                    _core.Logging.Write($"Connected on incoming tunnel '{Name}' on port {DataPort}");

                    using (_stream = client.GetStream())
                    {
                        ReseiveTunnelPackets(client);
                    }

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

        internal void SendStreamPacketMessage(NtPacketPayloadMessage message)
        {
            Utility.EnsureNotNull(_stream);
            NtPacketizer.SendStreamPacketMessage(_stream, message);
        }

        private void ReseiveTunnelPackets(TcpClient client)
        {
            Utility.EnsureNotNull(_stream);

            var packetBuffer = new NtPacketBuffer();

            while (_keepRunning)
            {
                SendStreamPacketMessage(new NtPacketPayloadMessage()
                {
                    Label = "This is the label.",
                    Message = "Message from inbound."
                });

                NtPacketizer.ReceiveAndProcessStreamPackets(_stream, this, packetBuffer, ProcessPacketCallbackHandler);

                Thread.Sleep(1000);
            }
            client.Close();
        }

        void ProcessPacketCallbackHandler(ITunnel tunnel, NtPacket packet)
        {
            if (packet.PayloadType == Packetizer.Constants.NtPayloadType.Message)
            {
                var message = NtPacketizer.ToObject<NtPacketPayloadMessage>(packet.Payload.Content);
                Debug.Print($"{message.Message}: {packet.CreatedTime}");
            }
        }
    }
}
