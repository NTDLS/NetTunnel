using NetTunnel.Library.Types;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening tunnel.
    /// </summary>
    public class TunnelOutbound : ITunnel
    {
        private readonly EngineCore _core;
        private readonly NtTunnelOutboundConfiguration _configuration;
        private Thread? _outgoingConnectionThread;
        private bool _keepRunning = false;

        private readonly List<EndpointInbound> _inboundEndpoints = new();
        private readonly List<EndpointOutbound> _outboundEndpoints = new();

        [JsonIgnore]
        public Guid PairId { get => _configuration.PairId; }
        [JsonIgnore]
        public string Name { get => _configuration.Name; }

        public TunnelOutbound(EngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            _core = core;
            _configuration = configuration;

            configuration.InboundEndpointConfigurations.ForEach(o => _inboundEndpoints.Add(new(_core, this, o)));
            configuration.OutboundEndpointConfigurations.ForEach(o => _outboundEndpoints.Add(new(_core, this, o)));
        }

        public NtTunnelOutboundConfiguration CloneConfiguration() => _configuration.Clone();

        public void AddEndpoint(NtEndpointInboundConfiguration configuration)
            => _inboundEndpoints.Add(new EndpointInbound(_core, this, configuration));

        public void AddEndpoint(NtEndpointOutboundConfiguration configuration)
            => _outboundEndpoints.Add(new EndpointOutbound(_core, this, configuration));

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing tunnel '{_configuration.Name}'");

            _outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            _outgoingConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private void OutgoingConnectionThreadProc()
        {
            while (_keepRunning)
            {
                try
                {
                    _core.Logging.Write($"Attempting to connect to outgoing tunnel '{_configuration.Name}' at {_configuration.Address}:{_configuration.DataPort}.");

                    var client = new TcpClient(_configuration.Address, _configuration.DataPort);

                    _core.Logging.Write($"Connection successful for tunnel '{_configuration.Name}' at {_configuration.Address}:{_configuration.DataPort}.");

                    HandleClient(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        void HandleClient(TcpClient client)
        {
            while (_keepRunning)
            {
                /*
                using (NetworkStream stream = client.GetStream())
                {
                    // Send data to the server.
                    string message = "Hello, server!";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // Receive data from the server.
                    byte[] receiveBuffer = new byte[1024];
                    int bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                    Console.WriteLine($"Received: {receivedMessage}");
                }
                */
                Thread.Sleep(10);
            }

            client.Close();
        }

    }
}
