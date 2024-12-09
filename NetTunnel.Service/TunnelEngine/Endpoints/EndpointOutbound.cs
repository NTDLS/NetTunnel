using System.Net.Sockets;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening endpoint.
    /// </summary>
    internal class EndpointOutbound : BaseEndpoint, IEndpoint
    {
        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey EndpointKey => new(this);
        public NtDirection Direction { get => NtDirection.Outbound; }
        public override int GetHashCode() => Configuration.GetHashCode();

        public EndpointOutbound(IServiceEngine serviceEngine, ITunnel tunnel, EndpointConfiguration configuration)
            : base(serviceEngine, tunnel, configuration.EndpointId, configuration)
        {
        }

        public override void Start()
        {
            base.Start();

            Singletons.Logger.Verbose(
                $"Starting outbound endpoint '{Configuration.Name}' on port {Configuration.OutboundPort}.");
        }

        public override void Stop()
        {
            base.Stop();

            Singletons.Logger.Verbose(
                $"Stopping outbound endpoint '{Configuration.Name}' on port {Configuration.OutboundPort}.");

            _edgeConnections.Use((o) =>
            {
                foreach (var activeConnection in o)
                {
                    Exceptions.Ignore(activeConnection.Value.Disconnect);
                }
                o.Clear();
            });

            Singletons.Logger.Verbose(
                $"Stopped outbound endpoint '{Configuration.Name}' on port {Configuration.OutboundPort}.");
        }

        public void EstablishOutboundEndpointConnection(Guid edgeId)
        {
            if (KeepRunning)
            {
                var tcpClient = new TcpClient();

                try
                {
                    tcpClient.Connect(Configuration.OutboundAddress, Configuration.OutboundPort);
                    if (KeepRunning) //Check again, we may have received a connection while shutting down.
                    {
                        var dataExchangeThread = new Thread(EndpointEdgeConnectionDataPumpThreadProc);
                        //Keep track of the connection. ActiveEndpointConnection will handle closing and disposing of the client and its stream.
                        var activeConnection = new EndpointEdgeConnection(dataExchangeThread, tcpClient, edgeId);
                        var outboundConnection = _edgeConnections.Use((o) => o.TryAdd(edgeId, activeConnection));

                        Singletons.Logger.Debug($"Established outbound endpoint connection: {activeConnection.EdgeId}");

                        dataExchangeThread.Start(activeConnection);
                    }
                }
                catch (Exception ex)
                {
                    Singletons.Logger.Exception(ex, $"EstablishOutboundEndpointConnection: {ex.Message}");
                    throw;
                }
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
                TrafficType = Configuration.TrafficType,
            };

            foreach (var rule in Configuration.HttpHeaderRules)
            {
                result.HttpHeaderRules.Add(rule.CloneConfiguration());
            }

            return result;
        }
    }
}
