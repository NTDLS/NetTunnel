using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class EndpointInbound : BaseEndpoint, IEndpoint
    {
        private TcpListener? _listener;
        private Thread? _inboundConnectionThread;

        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey EndpointKey => new(this);
        public NtDirection Direction { get => NtDirection.Inbound; }
        public override int GetHashCode() => Configuration.GetHashCode();

        public EndpointInbound(IServiceEngine serviceEngine, ITunnel tunnel, EndpointConfiguration configuration)
            : base(serviceEngine, tunnel, configuration.EndpointId, configuration)
        {
        }

        /// <summary>
        /// The certificate used to terminate TLS (HTTPS traffic type only).
        /// </summary>
        internal X509Certificate2? Certificate { get; private set; }

        public override void Start()
        {
            if (TryBeginStart() == false)
            {
                return; //Already running.
            }

            if (Configuration.TrafficType == NtTrafficType.Https)
            {
                try
                {
                    Certificate = X509CertificateLoader.LoadPkcs12FromFile(Configuration.TlsCertificatePath,
                        string.IsNullOrEmpty(Configuration.TlsCertificatePassword) ? null : Configuration.TlsCertificatePassword);
                }
                catch (Exception ex)
                {
                    //Don't listen at all: every connection would fail the TLS handshake anyway.
                    Singletons.Logger.Exception(ex, $"Inbound endpoint '{Configuration.Name}' will not listen on port {Configuration.InboundPort}"
                        + $" because the TLS certificate '{Configuration.TlsCertificatePath}' could not be loaded");
                    return;
                }
            }

            _listener = NetworkUtility.CreateListener(Configuration.InboundPort);

            Singletons.Logger.Verbose(
                $"Starting inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();
        }

        public override void Stop()
        {
            if (TryBeginStop() == false)
            {
                return; //Not running.
            }

            if (_listener != null)
            {
                Exceptions.Ignore(_listener.Stop);
                Exceptions.Ignore(_listener.Dispose);
                _listener = null;
            }

            if (_inboundConnectionThread != null && _inboundConnectionThread.ManagedThreadId != Environment.CurrentManagedThreadId)
            {
                _inboundConnectionThread.Join();
            }
            _inboundConnectionThread = null;

            Singletons.Logger.Verbose(
                $"Stopping inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            _edgeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Exceptions.Ignore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            Exceptions.Ignore(() => Certificate?.Dispose());
            Certificate = null;

            Singletons.Logger.Verbose(
                $"Stopped inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");
        }

        void InboundConnectionThreadProc()
        {
            Thread.CurrentThread.Name = $"InboundConnectionThreadProc:{Environment.CurrentManagedThreadId}";

            //Stop() clears the field, so hold onto our own reference.
            var listener = _listener.EnsureNotNull();

            try
            {
                try
                {
                    listener.Start();
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressFamilyNotSupported && listener.Server.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    //IPv6 is installed but not usable here (for example it is disabled on the network adapters). Listen on IPv4 only.
                    Singletons.Logger.Warning($"IPv6 is not available for inbound endpoint '{Configuration.Name}', listening on IPv4 only.");
                    Exceptions.Ignore(listener.Dispose);
                    listener = new TcpListener(IPAddress.Any, Configuration.InboundPort);
                    _listener = listener;
                    listener.Start();
                }

                Singletons.Logger.Verbose($"Listening inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort} ({(listener.Server.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv4 and IPv6" : "IPv4")})");

                while (KeepRunning)
                {
                    var tcpClient = listener.AcceptTcpClient(); //Wait for an inbound connection.

                    //Check again, we may have received a connection while shutting down.
                    if (tcpClient.Connected == false || KeepRunning == false)
                    {
                        Exceptions.Ignore(tcpClient.Dispose);
                        continue;
                    }

                    var endpointEdgeConnectionPumpThread = new Thread(EndpointEdgeConnectionDataPumpThreadProc);

                    //Keep track of the connection. ActiveEndpointConnection will handle closing and disposing of the client and its stream.
                    var activeConnection = new EndpointEdgeConnection(endpointEdgeConnectionPumpThread, tcpClient, Guid.NewGuid());
                    _edgeConnections.Use((o) => o.Add(activeConnection.EdgeId, activeConnection));

                    Singletons.Logger.Debug($"Accepted inbound endpoint connection: {activeConnection.EdgeId}");

                    endpointEdgeConnectionPumpThread.Start(activeConnection);
                }
            }
            catch (Exception ex)
            {
                if (KeepRunning) //Exceptions are expected when Stop() closes the listener.
                {
                    Singletons.Logger.Exception(ex, "InboundConnectionThreadProc");
                }
            }
            finally
            {
                Exceptions.Ignore(listener.Stop);
            }
        }

        public EndpointConfiguration GetForDisplay()
        {
            var result = new EndpointConfiguration
            {
                EndpointId = EndpointId,
                Direction = Direction,
                Name = Configuration.Name,
                OutboundAddress = Configuration.OutboundAddress,
                InboundPort = Configuration.InboundPort,
                OutboundPort = Configuration.OutboundPort,
                InboundTrafficType = Configuration.InboundTrafficType,
                OutboundTrafficType = Configuration.OutboundTrafficType,
                TlsCertificatePath = Configuration.TlsCertificatePath,
                TlsCertificatePassword = Configuration.TlsCertificatePassword,
                TlsIgnoreBackendCertificateErrors = Configuration.TlsIgnoreBackendCertificateErrors,
                AllowHttp2 = Configuration.AllowHttp2,
            };

            foreach (var rule in Configuration.HttpHeaderRules)
            {
                result.HttpHeaderRules.Add(rule.CloneConfiguration());
            }

            return result;
        }
    }
}
