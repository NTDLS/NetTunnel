using NetTunnel.Library.Types;
using NetTunnel.Service.Types;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class EndpointInbound : BaseEndpoint, IEndpoint
    {
        private Thread? _incomingConnectionThread;

        public int Port { get; private set; }

        public EndpointInbound(EngineCore core, ITunnel tunnel, NtEndpointInboundConfiguration configuration)
            : base(core, tunnel, configuration.PairId, configuration.Name)
        {
            Port = configuration.Port;
        }

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting incoming endpoint '{Name}' on port {Port}");

            _incomingConnectionThread = new Thread(IncomingConnectionThreadProc);
            _incomingConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        void IncomingConnectionThreadProc()
        {
            var tcpListener = new TcpListener(IPAddress.Any, Port);

            try
            {
                tcpListener.Start();

                _core.Logging.Write($"Listening incoming endpoint '{Name}' on port {Port}");

                while (_keepRunning)
                {
                    Guid streamId = Guid.NewGuid();

                    _core.Logging.Write($"Waiting for connection for incoming endpoint '{Name}' on port {Port}");

                    var tcpClient = tcpListener.AcceptTcpClient();

                    _core.Logging.Write($"Accepted on incoming endpoint '{Name}' on port {Port}");

                    var handlerThread = new Thread(HandleClientThreadProc);

                    var activeConnection = new ActiveEndpointConnection(handlerThread, tcpClient, streamId);

                    _activeConnections.Use((o) => o.Add(streamId, activeConnection));

                    handlerThread.Start(activeConnection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Stop listening and close the listener when done.
                tcpListener.Stop();
            }
        }
    }
}
