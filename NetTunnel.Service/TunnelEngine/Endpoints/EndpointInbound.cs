using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using System.Net;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class EndpointInbound : BaseEndpoint, IEndpoint
    {
        private Thread? _inboundConnectionThread;

        private TcpListener? _listener;

        public NtDirection Direction { get => NtDirection.Inbound; }

        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey EndpointKey => new(this);

        public override int GetHashCode()
            => Configuration.GetHashCode();

        public EndpointInbound(IServiceEngine serviceEngine, ITunnel tunnel, EndpointConfiguration configuration)
            : base(serviceEngine, tunnel, configuration.EndpointId, configuration)
        {
        }

        public override void Start()
        {
            base.Start();

            _listener = new TcpListener(IPAddress.Any, Configuration.InboundPort);

            _tunnel.ServiceEngine.Logger.Verbose(
                $"Starting inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (_listener != null)
            {
                Exceptions.Ignore(_listener.Stop);
                Exceptions.Ignore(_listener.Dispose);
            }

            _tunnel.ServiceEngine.Logger.Verbose(
                $"Stopping inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            _activeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Exceptions.Ignore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            _tunnel.ServiceEngine.Logger.Verbose(
                $"Stopped inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");
        }

        void InboundConnectionThreadProc()
        {
            Thread.CurrentThread.Name = $"InboundConnectionThreadProc:{Environment.CurrentManagedThreadId}";

            try
            {
                _listener.EnsureNotNull().Start();

                _serviceEngine.Logger.Verbose($"Listening inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}");

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

                            _serviceEngine.Logger.Debug($"Accepted inbound endpoint connection: {activeConnection.StreamId}");

                            dataExchangeThread.Start(activeConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _serviceEngine.Logger.Exception($"InboundConnectionThreadProc: {ex.Message}");
            }
            finally
            {
                if (_listener != null)
                {
                    Exceptions.Ignore(_listener.Stop);
                }
            }
        }
    }
}
