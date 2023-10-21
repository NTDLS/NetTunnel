using NetTunnel.Library.Types;
using Newtonsoft.Json;
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
        private readonly NtTunnelInboundConfiguration _configuration;
        private Thread? _incomingConnectionThread;
        private bool _keepRunning = false;

        private readonly List<EndpointInbound> _inboundEndpoints = new();
        private readonly List<EndpointOutbound> _outboundEndpoints = new();

        [JsonIgnore]
        public Guid PairId { get => _configuration.PairId; }
        [JsonIgnore]
        public string Name { get => _configuration.Name; }

        public TunnelInbound(EngineCore core, NtTunnelInboundConfiguration configuration)
        {
            _core = core;
            _configuration = configuration;

            configuration.InboundEndpointConfigurations.ForEach(o => _inboundEndpoints.Add(new(_core, this, o)));
            configuration.OutboundEndpointConfigurations.ForEach(o => _outboundEndpoints.Add(new(_core, this, o)));
        }

        public NtTunnelInboundConfiguration CloneConfiguration() => _configuration.Clone();

        public void AddEndpoint(NtEndpointInboundConfiguration configuration)
            => _inboundEndpoints.Add(new EndpointInbound(_core, this, configuration));

        public void AddEndpoint(NtEndpointOutboundConfiguration configuration)
            => _outboundEndpoints.Add(new EndpointOutbound(_core, this, configuration));

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting incoming tunnel '{_configuration.Name}' on port {_configuration.DataPort}");

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
            var listener = new TcpListener(IPAddress.Any, _configuration.DataPort);

            try
            {
                listener.Start();

                _core.Logging.Write($"Listening incoming tunnel '{_configuration.Name}' on port {_configuration.DataPort}");

                while (_keepRunning)
                {
                    _core.Logging.Write($"Waiting for connection for incoming tunnel '{_configuration.Name}' on port {_configuration.DataPort}");

                    var client = listener.AcceptTcpClient();
                    _core.Logging.Write($"Connected on incoming tunnel '{_configuration.Name}' on port {_configuration.DataPort}");

                    HandleClient(client);

                    _core.Logging.Write($"Disconnected incoming tunnel '{_configuration.Name}' on port {_configuration.DataPort}");

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

        void HandleClient(TcpClient client)
        {
            while (_keepRunning)
            {
                //TODO: Pump tunnel data.
                Thread.Sleep(10);
            }

            // Handle client communication here.
            // You can read and write data using the client's network stream.
            // Example:
            // NetworkStream stream = client.GetStream();
            // // Read and write data using the stream.
            // // Remember to close the client's resources when done.

            // Example of reading data from the client:
            // byte[] buffer = new byte[1024];
            // int bytesRead;
            // while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            // {
            //     // Process the received data.
            //     // You can also send data back to the client using stream.Write().
            // }

            // Close the client when done.
            client.Close();
        }

    }
}
