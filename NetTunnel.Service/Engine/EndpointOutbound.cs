using NetTunnel.Library.Types;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening endpoint.
    /// </summary>
    public class EndpointOutbound: IEndpoint
    {
        private readonly EngineCore _core;
        private NtEndpointOutboundConfiguration _configuration;
        private Thread? _outgoingConnectionThread;
        private bool _keepRunning = false;
        private readonly ITunnel _tunnel;

        public Guid PairId { get => _configuration.PairId; }
        public string Name { get => _configuration.Name; }

        public EndpointOutbound(EngineCore core, ITunnel tunnel, NtEndpointOutboundConfiguration configuration)
        {
            _core = core;
            _tunnel = tunnel;
            _configuration = configuration;
        }

        public NtEndpointOutboundConfiguration CloneConfiguration() => _configuration.Clone();

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing endpoint '{_configuration.Name}'");

            _outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            _outgoingConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
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

        private void HandleClient(TcpClient client)
        {
            while (_keepRunning)
            {
                /*
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //stream.Write().
                }
                */

                Thread.Sleep(1);
            }

            client.Close();
        }

    }
}
