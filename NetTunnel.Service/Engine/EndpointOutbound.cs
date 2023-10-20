using NetTunnel.Library.Types;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening endpoint.
    /// </summary>
    public class EndpointOutbound
    {
        private readonly EngineCore _core;
        private NtEndpointOutboundConfig _configuration;
        private Thread? _outgoingConnectionThread;
        private bool _keepRunning = false;

        public Guid Id { get => _configuration.Id; }

        public EndpointOutbound(EngineCore core, NtEndpointOutboundConfig config)
        {
            _core = core;
            _configuration = config;
        }

        public NtEndpointOutboundConfig CloneConfiguration() => _configuration.Clone();

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing endpoint '{_configuration.Name}'");

            _outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            _outgoingConnectionThread.Start();
        }

        public void Stop()
        {
        }

        void OutgoingConnectionThreadProc()
        {
            while (_keepRunning)
            {
                try
                {
                    _core.Logging.Write($"Attempting to connect to outgoing endpoint '{_configuration.Name}' at {_configuration.Address}:{_configuration.DataPort}.");

                    var client = new TcpClient(_configuration.Address, _configuration.DataPort);

                    _core.Logging.Write($"Connection successful for endpoint '{_configuration.Name}' at {_configuration.Address}:{_configuration.DataPort}.");

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
