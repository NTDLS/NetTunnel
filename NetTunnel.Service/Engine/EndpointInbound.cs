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

        public EndpointInbound(EngineCore core, ITunnel tunnel, NtEndpointInboundConfiguration configuration)
            : base(core, tunnel, configuration.PairId, configuration.Name)
        {
            Port = configuration.Port;
        }

        public void Start()
        {
            _core.Logging.Write($"Starting inbound endpoint '{Name}' on port {Port}");

            _keepRunning = true;

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        void InboundConnectionThreadProc()
        {
            var tcpListener = new TcpListener(IPAddress.Any, Port);

            try
            {
                tcpListener.Start();

                _core.Logging.Write($"Listening inbound endpoint '{Name}' on port {Port}");

                while (_keepRunning)
                {
                    var tcpClient = tcpListener.AcceptTcpClient(); //Wait for an inbound connection.

                    if (tcpClient.Connected)
                    {
                        var handlerThread = new Thread(HandleClientThreadProc);

                        //Keep track of the connection.
                        var activeConnection = new ActiveEndpointConnection(handlerThread, tcpClient, Guid.NewGuid());
                        _activeConnections.Use((o) => o.Add(activeConnection.StreamId, activeConnection));

                        handlerThread.Start(activeConnection);
                    }
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
