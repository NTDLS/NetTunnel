using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Tunnels;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class EndpointInbound : BaseEndpoint, IEndpoint
    {
        private Thread? _inboundConnectionThread;

        private readonly TcpListener _listener;

        public EndpointInbound(TunnelEngineCore core, ITunnel tunnel, NtEndpointInboundConfiguration configuration)
            : base(core, tunnel, configuration.PairId, configuration.Name, configuration.TransmissionPort)
        {
            _listener = new TcpListener(IPAddress.Any, configuration.TransmissionPort);
        }

        public override void Start()
        {
            base.Start();

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Starting inbound endpoint '{Name}' on port {TransmissionPort}.");

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();
        }

        public override void Stop()
        {
            base.Stop();

            Utility.TryAndIgnore(_listener.Stop);

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopping inbound endpoint '{Name}' on port {TransmissionPort}.");

            _activeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Utility.TryAndIgnore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopped inbound endpoint '{Name}' on port {TransmissionPort}.");
        }

        void InboundConnectionThreadProc()
        {
            Thread.CurrentThread.Name = $"InboundConnectionThreadProc:{Environment.CurrentManagedThreadId}";

            try
            {
                _listener.Start();

                _core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Listening inbound endpoint '{Name}' on port {TransmissionPort}");

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

                            _core.Logging.Write(Constants.NtLogSeverity.Debug, $"Accepted inbound endpoint connection: {activeConnection.StreamId}");
                            handlerThread.Start(activeConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _core.Logging.Write(Constants.NtLogSeverity.Exception, $"InboundConnectionThreadProc: {ex.Message}");
            }
            finally
            {
                Utility.TryAndIgnore(_listener.Stop);
            }
        }
    }
}
