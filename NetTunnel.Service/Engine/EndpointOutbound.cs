using NetTunnel.Library.Types;
using NetTunnel.Service.Types;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening endpoint.
    /// </summary>
    internal class EndpointOutbound : IEndpoint
    {
        private readonly EngineCore _core;
        private Thread? _outgoingConnectionThread;
        private bool _keepRunning = false;
        private readonly ITunnel _tunnel;

        public Guid PairId { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }

        public EndpointOutbound(EngineCore core, ITunnel tunnel, NtEndpointOutboundConfiguration configuration)
        {
            _core = core;
            _tunnel = tunnel;

            PairId = configuration.PairId;
            Name = configuration.Name;
            Address = configuration.Address;
            Port = configuration.Port;
        }

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing endpoint '{Name}'");

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
                    _core.Logging.Write($"Attempting to connect to outgoing endpoint '{Name}' at {Address}:{Port}.");

                    var client = new TcpClient(Address, Port);

                    _core.Logging.Write($"Connection successful for endpoint '{Name}' at {Address}:{Port}.");

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
