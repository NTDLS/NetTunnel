using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.TunnelEngine.Http2;
using NTDLS.Helpers;
using NTDLS.Semaphore;
using System.Net;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    internal class BaseEndpoint
    {
        private Thread? _heartbeatThread;
        private readonly object _lifecycleLock = new();
        protected readonly object _statisticsLock = new();

        internal readonly PessimisticCriticalResource<Dictionary<Guid, IEdgeConnection>> _edgeConnections = new();
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
                        List<IEdgeConnection> connectionsToClose = new();

                        foreach (var connection in o)
                        {
                            Exceptions.Ignore(() =>
                            {
                                //We've are connected but haven't done much in a while. UDP has no connection that closes, so a session
                                //  which stops sending is the only way it ever ends: it always has a limit.
                                double limit = Configuration.IsUdp
                                    ? Singletons.Configuration.UdpSessionTimeoutMs
                                    : Singletons.Configuration.StaleEndpointExpirationMs;

                                if (limit > 0 && connection.Value.ActivityAgeInMilliseconds > limit)
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

        /// <summary>
        /// Marks the endpoint as running and starts the heartbeat thread.
        /// Returns false if the endpoint was already running so that derived classes can skip their own startup.
        /// </summary>
        protected bool TryBeginStart()
        {
            lock (_lifecycleLock)
            {
                if (KeepRunning)
                {
                    return false;
                }

                //KeepRunning must be set before the heartbeat thread starts, otherwise its loop exits immediately.
                KeepRunning = true;
                _heartbeatThread = new Thread(HeartbeatThreadProc);
                _heartbeatThread.Start();
                return true;
            }
        }

        /// <summary>
        /// Marks the endpoint as stopped and waits on the heartbeat thread.
        /// Returns false if the endpoint was not running so that derived classes can skip their own shutdown.
        /// </summary>
        protected bool TryBeginStop()
        {
            Thread? heartbeatThread;

            lock (_lifecycleLock)
            {
                if (KeepRunning == false)
                {
                    return false;
                }

                KeepRunning = false;
                heartbeatThread = _heartbeatThread;
                _heartbeatThread = null;
            }

            if (heartbeatThread != null && heartbeatThread.ManagedThreadId != Environment.CurrentManagedThreadId)
            {
                heartbeatThread.Join();
            }

            return true;
        }

        protected void AddBytesReceived(int bytes)
        {
            lock (_statisticsLock)
            {
                BytesReceived += (ulong)bytes;
            }
        }

        protected void NoteConnectionOpened()
        {
            lock (_statisticsLock)
            {
                TotalConnections++;
                CurrentConnections++;
            }
        }

        protected void NoteConnectionClosed()
        {
            lock (_statisticsLock)
            {
                if (CurrentConnections > 0)
                {
                    CurrentConnections--;
                }
            }
        }

        /// <summary>
        /// The configuration of the endpoint as it is shown to the UI.
        /// </summary>
        protected EndpointConfiguration BuildDisplayConfiguration(NtDirection direction)
        {
            var result = new EndpointConfiguration
            {
                EndpointId = EndpointId,
                Direction = direction,
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

        public virtual void Start() => TryBeginStart();

        public virtual void Stop() => TryBeginStop();

        /// <param name="packetCount">
        /// When the peer is the one disconnecting, the number of data packets it sent first. They are delivered before the connection is closed.
        /// Negative closes the connection at once.
        /// </param>
        public void Disconnect(Guid edgeId, long packetCount = -1)
        {
            if (packetCount >= 0)
            {
                var edgeConnection = _edgeConnections.Use((o) => o.GetValueOrDefault(edgeId));
                if (edgeConnection != null)
                {
                    //The data pump removes the connection once it sees the end, so it is left in place until then.
                    Exceptions.Ignore(() => edgeConnection.DisconnectAfter(packetCount));
                }
                return;
            }

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

        public void WriteEndpointEdgeData(Guid edgeId, long packetSequence, byte[] buffer)
        {
            var edgeConnection = _edgeConnections.Use((o) =>
            {
                o.TryGetValue(edgeId, out var edgeConnection);
                return edgeConnection;
            });

            if (edgeConnection == null)
            {
                return; //The edge connection has already closed; discard the data.
            }

            lock (_statisticsLock)
            {
                BytesSent += (ulong)buffer.Length;
            }

            try
            {
                edgeConnection.Write(packetSequence, buffer);
            }
            catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
            {
                //The client (browser, server, etc.) went away while we were delivering to it. That is not a fault in the
                //  service, but the connection is unusable, so close it. The data pump will notify the peer when it unwinds.
                Singletons.Logger.Verbose($"Write to edge connection {edgeId} failed, closing it: {ex.Message}");
                Disconnect(edgeId);
            }
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

            long packetSequence = -1;
            bool inTlsHandshake = false;

            try
            {
                if (Configuration.TrafficType == NtTrafficType.Https && this is EndpointInbound inboundEndpoint)
                {
                    //Terminate TLS here so that the tunneled data (and HTTP header rules) are in the clear.
                    //  This must complete before we notify the peer, otherwise it could write to us mid-handshake.
                    inTlsHandshake = true;
                    edgeConnection.AuthenticateAsTlsServer(inboundEndpoint.Certificate
                        ?? throw new Exception($"No TLS certificate is loaded for endpoint '{Configuration.Name}'."), Configuration.AllowHttp2);
                    inTlsHandshake = false;
                }

                if (Configuration.Direction == NtDirection.Inbound)
                {
                    //SEARCH FOR: Process:Endpoint:Connect:001: If this is an inbound endpoint, then let the remote service
                    //  know that we just received a connection so that it came make the associated outbound connection.
                    _tunnel.S2SPeerNotificationEndpointConnect(_tunnel.TunnelKey, EndpointId, edgeConnection.EdgeId, edgeConnection.ApplicationProtocol);
                }

                //Only follow the HTTP stream if the traffic type is HTTP(S) and there are rules for what this end reads:
                //  requests at the inbound endpoint and responses at the outbound endpoint.
                IHttpStreamProcessor? httpParser = (Configuration.TrafficType == NtTrafficType.Http || Configuration.TrafficType == NtTrafficType.Https)
                    && Configuration.HttpHeaderRules.Any(o => o.Enabled
                        && (o.HeaderType == NtHttpHeaderType.Any
                            || (this is EndpointInbound && o.HeaderType == NtHttpHeaderType.Request)
                            || (this is EndpointOutbound && o.HeaderType == NtHttpHeaderType.Response)))
                    ? (edgeConnection.ApplicationProtocol == "h2"
                        ? new Http2StreamParser(Configuration, fromClient: this is EndpointInbound)
                        : new HttpStreamParser(Configuration))
                    : null;

                var buffer = new ReceiveBuffer(Singletons.Configuration.InitialReceiveBufferSize);

                while (KeepRunning && edgeConnection.IsConnected && edgeConnection.Read(ref buffer))
                {
                    lock (_statisticsLock)
                    {
                        BytesReceived += (ulong)buffer.Length;
                    }

                    var (sendBytes, sendLength) = httpParser != null
                        ? httpParser.Process(buffer.Bytes, buffer.Length)
                        : (buffer.Bytes, buffer.Length);

                    if (sendLength > 0) //Nothing to send while the parser is holding a partial HTTP header.
                    {
                        //Send the data to the remote peer, along with all the IDs required to identify the tunnel,
                        //  endpoint and endpoint-edge-connection (edgeId). At the tunnel-peer, This data will be
                        //  sent to whatever is connected to the endpoint via a call to WriteEndpointEdgeData().
                        //  Sequence numbers must be contiguous, so only consume one when a packet is actually sent. The count is
                        //  what tells the peer how many packets to wait for when we disconnect.
                        _tunnel.S2SPeerNotificationEndpointDataExchange(
                            _tunnel.TunnelKey, Configuration.EndpointId, edgeConnection.EdgeId, packetSequence + 1, sendBytes, sendLength);
                        packetSequence++;
                    }

                    buffer.AutoResize(Singletons.Configuration.MaxReceiveBufferSize);
                }
            }
            catch (Exception ex) when (inTlsHandshake && ex is IOException or System.Security.Authentication.AuthenticationException)
            {
                //Browsers routinely open speculative connections and drop them without completing the handshake, or reject
                //  a certificate they don't trust. Neither is a fault in the service, so no stack trace.
                var reason = ex.InnerException?.Message ?? ex.Message;
                if (ex is System.Security.Authentication.AuthenticationException)
                {
                    Singletons.Logger.Warning($"TLS handshake failed on inbound endpoint '{Configuration.Name}' (port {Configuration.InboundPort}): {reason}");
                }
                else
                {
                    Singletons.Logger.Verbose($"TLS handshake on inbound endpoint '{Configuration.Name}' (port {Configuration.InboundPort}) ended by the client: {ex.Message}");
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
                        Singletons.Logger.Exception(ex, "EndpointEdgeConnectionDataPumpThread");
                    }
                }
                else
                {
                    Singletons.Logger.Exception(ex, "EndpointEdgeConnectionDataPumpThread");
                }
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex, "EndpointEdgeConnectionDataPumpThread");
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

            //Tell the peer how many packets we sent, because it handles notifications concurrently and the disconnect can otherwise
            //  overtake the last of them, truncating the transfer.
            Exceptions.Ignore(() =>
                _tunnel.S2SPeerNotificationEndpointDisconnect(_tunnel.TunnelKey.SwapDirection(), EndpointId, edgeConnection.EdgeId, packetSequence + 1));
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
                        ThreadId = edge.ThreadId
                    };

                    if (edge.RemoteEndPoint is IPEndPoint address)
                    {
                        var remote = Utility.Unmap(address.Address);
                        connection.AddressFamily = remote.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4";
                        connection.Address = remote.ToString();
                        connection.Port = address.Port;
                    }

                    connections.Add(connection);
                }
            });

            return connections;
        }
    }
}
