﻿using NetTunnel.Library.Interfaces;
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

        public override void Start()
        {
            base.Start();

            _listener = new TcpListener(IPAddress.Any, Configuration.InboundPort);

            Singletons.Logger.Verbose(
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

            Singletons.Logger.Verbose(
                $"Stopped inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");
        }

        void InboundConnectionThreadProc()
        {
            Thread.CurrentThread.Name = $"InboundConnectionThreadProc:{Environment.CurrentManagedThreadId}";

            try
            {
                _listener.EnsureNotNull().Start();

                Singletons.Logger.Verbose($"Listening inbound endpoint '{Configuration.Name}' on port {Configuration.InboundPort}");

                while (KeepRunning)
                {
                    var tcpClient = _listener.AcceptTcpClient(); //Wait for an inbound connection.

                    if (tcpClient.Connected)
                    {
                        if (KeepRunning) //Check again, we may have received a connection while shutting down.
                        {
                            var endpointEdgeConnectionPumpThread = new Thread(EndpointEdgeConnectionDataPumpThreadProc);

                            //Keep track of the connection. ActiveEndpointConnection will handle closing and disposing of the client and its stream.
                            var activeConnection = new EndpointEdgeConnection(endpointEdgeConnectionPumpThread, tcpClient, Guid.NewGuid());
                            _edgeConnections.Use((o) => o.Add(activeConnection.EdgeId, activeConnection));

                            Singletons.Logger.Debug($"Accepted inbound endpoint connection: {activeConnection.EdgeId}");

                            endpointEdgeConnectionPumpThread.Start(activeConnection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("blocking operation was interrupted", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    Singletons.Logger.Exception($"InboundConnectionThreadProc: {ex.Message}");
                }
            }
            finally
            {
                if (_listener != null)
                {
                    Exceptions.Ignore(_listener.Stop);
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
