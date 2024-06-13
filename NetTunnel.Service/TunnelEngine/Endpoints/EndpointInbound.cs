using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.NullExtensions;
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

        private TcpListener? _listener;

        public EndpointInbound(TunnelEngineCore core, ITunnel tunnel, NtEndpointInboundConfiguration configuration)
            : base(core, tunnel, configuration.EndpointId, configuration)
        {
        }

        public override void Start()
        {
            base.Start();

            _listener = new TcpListener(IPAddress.Any, Configuration.InboundPort);

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose,
                $"Starting inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (_listener != null)
            {
                Utility.TryAndIgnore(_listener.Stop);
                Utility.TryAndIgnore(_listener.Dispose);
            }

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose,
                $"Stopping inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            _activeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Utility.TryAndIgnore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose,
                $"Stopped inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");
        }

        void InboundConnectionThreadProc()
        {
            Thread.CurrentThread.Name = $"InboundConnectionThreadProc:{Environment.CurrentManagedThreadId}";

            try
            {
                _listener.EnsureNotNull().Start();

                _core.Logging.Write(Constants.NtLogSeverity.Verbose,
                    $"Listening inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}");

                while (KeepRunning)
                {
                    var tcpClient = _listener.AcceptTcpClient(); //Wait for an inbound connection.

                    if (tcpClient.Connected)
                    {
                        if (KeepRunning) //Check again, we may have received a connection while shutting down.
                        {
                            var dataExchangeThread = new Thread(EndpointDataExchangeThreadProc);

                            //Keep track of the connection. ActiveEndpointConnection will handle closing and disposing of the client and its stream.
                            var activeConnection = new ActiveEndpointConnection(dataExchangeThread, tcpClient, Guid.NewGuid());
                            _activeConnections.Use((o) => o.Add(activeConnection.StreamId, activeConnection));

                            _core.Logging.Write(Constants.NtLogSeverity.Debug,
                                $"Accepted inbound endpoint connection: {activeConnection.StreamId}");
                            dataExchangeThread.Start(activeConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _core.Logging.Write(Constants.NtLogSeverity.Exception,
                    $"InboundConnectionThreadProc: {ex.Message}");
            }
            finally
            {
                if (_listener != null)
                {
                    Utility.TryAndIgnore(_listener.Stop);
                }
            }
        }
    }
}
