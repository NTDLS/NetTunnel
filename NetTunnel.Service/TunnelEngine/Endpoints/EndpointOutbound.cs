using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening endpoint.
    /// </summary>
    internal class EndpointOutbound : BaseEndpoint, IEndpoint
    {
        public override int GetHashCode()
            => Configuration.GetHashCode();

        public NtDirection Direction { get => NtDirection.Outbound; }

        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey EndpointKey => new(this);

        public EndpointOutbound(IServiceEngine serviceEngine, ITunnel tunnel, EndpointConfiguration configuration)
            : base(serviceEngine, tunnel, configuration.EndpointId, configuration)
        {
        }

        public override void Start()
        {
            base.Start();

            _tunnel.ServiceEngine.Logger.Verbose(
                $"Starting outbound endpoint '{Configuration.Name}' on port {Configuration.OutboundPort}.");
        }

        public override void Stop()
        {
            base.Stop();

            _tunnel.ServiceEngine.Logger.Verbose(
                $"Stopping outbound endpoint '{Configuration.Name}' on port {Configuration.OutboundPort}.");

            _activeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Exceptions.Ignore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            _tunnel.ServiceEngine.Logger.Verbose(
                $"Stopped outbound endpoint '{Configuration.Name}' on port {Configuration.OutboundPort}.");
        }

        public void EstablishOutboundEndpointConnection(Guid streamId)
        {
            if (KeepRunning)
            {
                var tcpClient = new TcpClient();

                try
                {
                    tcpClient.Connect(Configuration.OutboundAddress, Configuration.OutboundPort);
                    if (KeepRunning) //Check again, we may have received a connection while shutting down.
                    {
                        var dataExchangeThread = new Thread(EndpointDataExchangeThreadProc);
                        //Keep track of the connection. ActiveEndpointConnection will handle closing and disposing of the client and its stream.
                        var activeConnection = new ActiveEndpointConnection(dataExchangeThread, tcpClient, streamId);
                        var outboundConnection = _activeConnections.Use((o) => o.TryAdd(streamId, activeConnection));

                        _serviceEngine.Logger.Debug($"Established outbound endpoint connection: {activeConnection.StreamId}");

                        dataExchangeThread.Start(activeConnection);
                    }
                }
                catch (Exception ex)
                {
                    _serviceEngine.Logger.Exception(ex, $"EstablishOutboundEndpointConnection: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
