using NetTunnel.Library;
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
        private Thread? _inboundConnectionThread;

        public int Port { get; private set; }

        private TcpListener _listener;

        public EndpointInbound(EngineCore core, ITunnel tunnel, NtEndpointInboundConfiguration configuration)
            : base(core, tunnel, configuration.PairId, configuration.Name)
        {
            Port = configuration.Port;

            _listener = new TcpListener(IPAddress.Any, Port);
        }

        public override void Start()
        {
            base.Start();

            _tunnel.Core.Logging.Write($"Starting inbound endpoint '{Name}' on port {Port}.");

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();
        }

        public override void Stop()
        {
            base.Stop();

            Utility.TryAndIgnore(_listener.Stop);

            _tunnel.Core.Logging.Write($"Stopping inbound endpoint '{Name}' on port {Port}.");

            _activeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Utility.TryAndIgnore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            _tunnel.Core.Logging.Write($"Stopped inbound endpoint '{Name}' on port {Port}.");
        }

        void InboundConnectionThreadProc()
        {
            try
            {
                _listener.Start();

                _core.Logging.Write($"Listening inbound endpoint '{Name}' on port {Port}");

                while (KeepRunning)
                {
                    var tcpClient = _listener.AcceptTcpClient(); //Wait for an inbound connection.

                    if (tcpClient.Connected)
                    {
                        if (KeepRunning) //Check again, we may have received a connection while shutting down.
                        {
                            var handlerThread = new Thread(HandleClientThreadProc);

                            //Keep track of the connection. ActiveEndpointConnection will handle closing and disposing of the client and its stream.
                            var activeConnection = new ActiveEndpointConnection(handlerThread, tcpClient, Guid.NewGuid());
                            _activeConnections.Use((o) => o.Add(activeConnection.StreamId, activeConnection));

                            handlerThread.Start(activeConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception[InboundConnectionThreadProc]: {ex.Message}");
            }
            finally
            {
                Utility.TryAndIgnore(_listener.Stop);
            }
        }
    }
}
