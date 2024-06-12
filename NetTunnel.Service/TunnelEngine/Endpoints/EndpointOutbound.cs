using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Tunnels;
using System.Net.Sockets;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening endpoint.
    /// </summary>
    internal class EndpointOutbound : BaseEndpoint, IEndpoint
    {
        public string Address { get; private set; }

        public NtEndpointOutboundConfiguration Configuration { get; private set; }

        public EndpointOutbound(TunnelEngineCore core, ITunnel tunnel, NtEndpointOutboundConfiguration configuration)
            : base(core, tunnel, configuration.EndpointId, configuration.Name, configuration.TransmissionPort)
        {
            Configuration = configuration;
            Address = configuration.Address;
        }

        public override void Start()
        {
            base.Start();

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Starting outbound endpoint '{Name}' on port {TransmissionPort}.");
        }

        public override void Stop()
        {
            base.Stop();

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopping outbound endpoint '{Name}' on port {TransmissionPort}.");

            _activeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Utility.TryAndIgnore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"Stopped outbound endpoint '{Name}' on port {TransmissionPort}.");
        }

        public void EstablishOutboundEndpointConnection(Guid streamId)
        {
            if (KeepRunning)
            {
                var tcpClient = new TcpClient();

                try
                {
                    tcpClient.Connect(Address, TransmissionPort);
                    if (KeepRunning) //Check again, we may have received a connection while shutting down.
                    {
                        var dataExchangeThread = new Thread(EndpointDataExchangeThreadProc);
                        //Keep track of the connection. ActiveEndpointConnection will handle closing and disposing of the client and its stream.
                        var activeConnection = new ActiveEndpointConnection(dataExchangeThread, tcpClient, streamId);
                        var outboundConnection = _activeConnections.Use((o) => o.TryAdd(streamId, activeConnection));

                        _core.Logging.Write(Constants.NtLogSeverity.Debug, $"Established outbound endpoint connection: {activeConnection.StreamId}");

                        dataExchangeThread.Start(activeConnection);
                    }
                }
                catch (Exception ex)
                {
                    _core.Logging.Write(Constants.NtLogSeverity.Exception, $"EstablishOutboundEndpointConnection: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
