using NetTunnel.Library.Types;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    public class EndpointInbound
    {
        private readonly EngineCore _core;
        private NtEndpointInboundConfiguration _configuration;
        private Thread? _incomingConnectionThread;
        private bool _keepRunning = false;

        public Guid Id { get => _configuration.Id; }

        public EndpointInbound(EngineCore core, NtEndpointInboundConfiguration config)
        {
            _core = core;
            _configuration = config;
        }

        public NtEndpointInboundConfiguration CloneConfiguration() => _configuration.Clone();

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting incoming endpoint '{_configuration.Name}' on port {_configuration.DataPort}");

            _incomingConnectionThread = new Thread(IncomingConnectionThreadProc);
            _incomingConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread to stop.
        }

        void IncomingConnectionThreadProc()
        {
            var listener = new TcpListener(IPAddress.Any, _configuration.DataPort);

            try
            {
                listener.Start();

                _core.Logging.Write($"Listening incoming endpoint '{_configuration.Name}' on port {_configuration.DataPort}");

                while (_keepRunning)
                {
                    _core.Logging.Write($"Waiting for connection for incoming endpoint '{_configuration.Name}' on port {_configuration.DataPort}");

                    var client = listener.AcceptTcpClient();
                    _core.Logging.Write($"Connected on incoming endpoint '{_configuration.Name}' on port {_configuration.DataPort}");

                    HandleClient(client);

                    _core.Logging.Write($"Disconnected incoming endpoint '{_configuration.Name}' on port {_configuration.DataPort}");

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
                //TODO: Pump endpoint data.
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
