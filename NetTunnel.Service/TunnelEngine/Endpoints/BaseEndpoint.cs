using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.Semaphore;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    internal class BaseEndpoint
    {
        private readonly Thread _heartbeatThread;
        private readonly object _statisticsLock = new();

        internal readonly PessimisticCriticalResource<Dictionary<Guid, EndpointEdgeConnection>> _edgeConnections = new();
        internal readonly ITunnel _tunnel;
        internal readonly IServiceEngine _serviceEngine;

        public bool KeepRunning { get; internal set; } = false;
        public EndpointConfiguration Configuration { get; private set; }
        public Guid EndpointId { get; private set; }
        public ulong BytesReceived { get; internal set; }
        public ulong BytesSent { get; internal set; }
        public ulong CurrentConnections { get; internal set; }
        public ulong TotalConnections { get; internal set; }

        public BaseEndpoint(IServiceEngine serviceEngine, ITunnel tunnel,
            Guid endpointId, EndpointConfiguration configuration)
        {
            Configuration = configuration;

            _serviceEngine = serviceEngine;
            _tunnel = tunnel;
            EndpointId = endpointId;
            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        private void HeartbeatThreadProc()
        {
            Thread.CurrentThread.Name = $"HeartbeatThreadProc:{Environment.CurrentManagedThreadId}";

            var lastHeartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastHeartBeat).TotalMilliseconds > Singletons.Configuration.EndpointHeartbeatDelayMs)
                {
                    _edgeConnections.Use((o) =>
                    {
                        List<EndpointEdgeConnection> connectionsToClose = new();

                        foreach (var connection in o)
                        {
                            Exceptions.Ignore(() =>
                            {
                                //We've are connected but haven't done much in a while.
                                if (Singletons.Configuration.StaleEndpointExpirationMs > 0
                                    && connection.Value.ActivityAgeInMilliseconds > Singletons.Configuration.StaleEndpointExpirationMs)
                                {
                                    connectionsToClose.Add(connection.Value);
                                }
                            });
                        }

                        foreach (var connection in connectionsToClose)
                        {
                            Exceptions.Ignore(connection.Disconnect);
                            Exceptions.Ignore(() => o.Remove(connection.EdgeId));
                        }
                    });

                    lastHeartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }

        public virtual void Start()
        {
            KeepRunning = true;
        }

        public virtual void Stop()
        {
            KeepRunning = false;
            _heartbeatThread.Join();
        }

        public void Disconnect(Guid edgeId)
        {
            _edgeConnections.Use((o) =>
            {
                if (o.TryGetValue(edgeId, out var activeEndpointConnection))
                {
                    Exceptions.Ignore(activeEndpointConnection.Disconnect);
                    Exceptions.Ignore(activeEndpointConnection.Dispose);
                    o.Remove(edgeId);
                }
            });
        }

        public void WriteEndpointEdgeData(Guid edgeId, byte[] buffer)
        {
            lock (_statisticsLock)
            {
                BytesSent += (ulong)buffer.Length;
            }

            var edgeConnection = _edgeConnections.Use((o) =>
            {
                o.TryGetValue(edgeId, out var edgeConnection);
                return edgeConnection;
            });

            edgeConnection?.Write(buffer);
        }

        /// <summary>
        /// This is where data is received by the endpoint client connections (e.g. web-browser, web-server, etc).
        /// </summary>
        /// <param name="obj"></param>
        internal void EndpointEdgeConnectionDataPumpThreadProc(object? obj)
        {
            Thread.CurrentThread.Name = $"EndpointEdgeConnectionDataPumpThread:{Environment.CurrentManagedThreadId}";

            var edgeConnection = ((EndpointEdgeConnection?)obj).EnsureNotNull();

            lock (_statisticsLock)
            {
                TotalConnections++;
                CurrentConnections++;
            }

            try
            {
                if (Configuration.Direction == NtDirection.Inbound)
                {
                    //SEARCH FOR: Process:Endpoint:Connect:001: If this is an inbound endpoint, then let the remote service
                    //  know that we just received a connection so that it came make the associated outbound connection.
                    _tunnel.S2SPeerNotificationEndpointConnect(_tunnel.TunnelKey, EndpointId, edgeConnection.EdgeId);
                }

                var httpHeaderBuilder = new StringBuilder();

                var buffer = new PumpBuffer(Singletons.Configuration.InitialReceiveBufferSize);

                while (KeepRunning && edgeConnection.IsConnected && edgeConnection.Read(ref buffer))
                {
                    lock (_statisticsLock)
                    {
                        BytesReceived += (ulong)buffer.Length;
                    }

                    #region HTTP Header Augmentation.

                    if (
                         //Only parse HTTP headers if the traffic type is HTTP.
                         Configuration.TrafficType == NtTrafficType.Http
                         &&
                         (
                            // and the direction is inbound/any and we have request rules.
                            (
                             this is EndpointInbound && Configuration.HttpHeaderRules
                                .Any(o => o.Enabled && (new[] { NtHttpHeaderType.Request, NtHttpHeaderType.Any }).Contains(o.HeaderType))
                            )
                            ||
                            (
                                // or the direction is outbound/any and we have response rules.
                                this is EndpointOutbound && Configuration.HttpHeaderRules
                                    .Any(o => o.Enabled && (new[] { NtHttpHeaderType.Request, NtHttpHeaderType.Any }).Contains(o.HeaderType))
                            )
                         )
                     )
                    {
                        switch (HttpUtility.Process(ref httpHeaderBuilder, Configuration, buffer))
                        {
                            case NtHTTPHeaderResult.WaitOnData:
                                //We received a partial HTTP header, wait on more data.
                                continue;
                            case NtHTTPHeaderResult.Present:
                                //Found an HTTP header, send it to the peer. There may be bytes remaining
                                //  in the buffer if buffer.Length > 0 so follow though to WriteBytesToPeer.

                                var httpHeaderBytes = Encoding.UTF8.GetBytes(httpHeaderBuilder.ToString());

                                _tunnel.S2SPeerNotificationEndpointDataExchange(
                                    _tunnel.TunnelKey, EndpointId, edgeConnection.EdgeId, httpHeaderBytes, httpHeaderBytes.Length);

                                httpHeaderBuilder.Clear();
                                break;
                            case NtHTTPHeaderResult.NotPresent:
                                //Didn't find an HTTP header.
                                break;
                        }
                    }

                    #endregion

                    //Send the data to the remote peer, along with all the IDs required to identify the tunnel,
                    //  endpoint and endpoint-edge-connection (edgeId). At the tunnel-peer, This data will be
                    //  sent to whatever is connected to the endpoint via a call to WriteEndpointEdgeData().
                    _tunnel.S2SPeerNotificationEndpointDataExchange(
                        _tunnel.TunnelKey, Configuration.EndpointId, edgeConnection.EdgeId, buffer.Bytes, buffer.Length);

                    buffer.AutoResize(Singletons.Configuration.MaxReceiveBufferSize);
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException sockEx)
                {
                    if (sockEx.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        //We don't typically care about this. This is something as simple as a user closing a web-browser.
                        Singletons.Logger.Verbose($"EndpointEdgeConnectionDataPumpThread: {ex.Message}");
                    }
                    else
                    {
                        Singletons.Logger.Exception($"EndpointEdgeConnectionDataPumpThread: {ex.Message}");
                    }
                }
                else
                {
                    Singletons.Logger.Exception($"EndpointEdgeConnectionDataPumpThread: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception($"EndpointEdgeConnectionDataPumpThread: {ex.Message}");
            }
            finally
            {
                lock (_statisticsLock)
                {
                    CurrentConnections--;
                }

                Exceptions.Ignore(edgeConnection.Disconnect);

                _edgeConnections.Use((o) =>
                {
                    o.Remove(edgeConnection.EdgeId);
                });
            }

            Exceptions.Ignore(() =>
                _tunnel.S2SPeerNotificationEndpointDisconnect(_tunnel.TunnelKey.SwapDirection(), EndpointId, edgeConnection.EdgeId));
        }

        public EndpointPropertiesDisplay GetProperties()
        {
            var props = new EndpointPropertiesDisplay()
            {
                BytesReceived = BytesReceived,
                BytesSent = BytesSent,
                TotalConnections = TotalConnections,
                CurrentConnections = CurrentConnections,
                EndpointId = EndpointId,
                KeepRunning = KeepRunning,
                Direction = Configuration.Direction,
                TunnelKey = _tunnel.TunnelKey,
                EndpointKey = new(EndpointId, Configuration.Direction),
                TrafficType = Configuration.TrafficType,
                Name = Configuration.Name,
                OutboundAddress = Configuration.OutboundAddress,
                InboundPort = Configuration.InboundPort,
                OutboundPort = Configuration.OutboundPort,
                HttpHeaderRules = Configuration.HttpHeaderRules.Count
            };

            return props;
        }

        public List<EndpointEdgeConnectionDisplay> GetEdgeConnections()
        {
            var connections = new List<EndpointEdgeConnectionDisplay>();

            _edgeConnections.Use(o =>
            {
                foreach (var edge in o.Values)
                {
                    var connection = new EndpointEdgeConnectionDisplay()
                    {
                        TunnelKey = _tunnel.TunnelKey,
                        EndpointKey = new DirectionalKey(Configuration.EndpointId, Configuration.Direction),
                        BytesReceived = edge.BytesReceived,
                        BytesSent = edge.BytesSent,
                        EdgeId = edge.EdgeId,
                        IsConnected = edge.IsConnected,
                        LastActivityDateTime = edge.LastActivityDateTime,
                        StartDateTime = edge.StartDateTime,
                        ThreadId = edge.Thread.ManagedThreadId
                    };

                    if (edge.TcpClient.Client.RemoteEndPoint is IPEndPoint address)
                    {
                        connection.AddressFamily = address.AddressFamily.ToString();
                        connection.Address = address.Address.ToString();
                        connection.Port = address.Port;
                    }

                    connections.Add(connection);
                }
            });
            return connections;
        }
    }
}
