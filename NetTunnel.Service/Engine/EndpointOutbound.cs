using NetTunnel.Library.Types;
using NetTunnel.Service.Types;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening endpoint.
    /// </summary>
    internal class EndpointOutbound : BaseEndpoint, IEndpoint
    {
        public string Address { get; private set; }
        public int Port { get; private set; }

        public EndpointOutbound(EngineCore core, ITunnel tunnel, NtEndpointOutboundConfiguration configuration)
            : base(core, tunnel, configuration.PairId, configuration.Name)
        {
            Address = configuration.Address;
            Port = configuration.Port;
        }

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing endpoint '{Name}'");

            //_outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            //_outgoingConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        public void StartConnection(Guid streamId)
        {
            var tcpClient = new TcpClient();

            try
            {
                _core.Logging.Write($"Connecting outbound endpoint '{Name}' on port {Port}"); ;

                tcpClient.Connect(Address, Port);

                _core.Logging.Write($"Accepted on incoming endpoint '{Name}' on port {Port}");

                var handlerThread = new Thread(HandleClientThreadProc);
                var param = new ActiveEndpointConnection(handlerThread, tcpClient, streamId);

                var outboundConnection = _activeConnections.Use((o) => o.TryAdd(streamId, param));

                handlerThread.Start(param);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
