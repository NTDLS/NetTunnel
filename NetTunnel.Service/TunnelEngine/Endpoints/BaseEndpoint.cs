using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using NTDLS.Semaphore;
using System.Net.Sockets;
using System.Text;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    internal class BaseEndpoint
    {
        private readonly object _statisticsLock = new();

        public ulong BytesReceived { get; internal set; }
        public ulong BytesSent { get; internal set; }
        public ulong TotalConnections { get; internal set; }
        public ulong CurrentConnections { get; internal set; }
        public Guid EndpointId { get; private set; }

        internal readonly IServiceEngine _serviceEngine;
        internal readonly ITunnel _tunnel;
        public bool KeepRunning { get; internal set; } = false;

        private readonly Thread _heartbeatThread;

        internal readonly PessimisticCriticalResource<Dictionary<Guid, ActiveEndpointConnection>> _activeConnections = new();

        public EndpointConfiguration Configuration { get; private set; }

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

            DateTime lastHeartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastHeartBeat).TotalMilliseconds > Singletons.Configuration.TunnelAndEndpointHeartbeatDelayMs)
                {
                    _activeConnections.Use((o) =>
                    {
                        List<ActiveEndpointConnection> connectionsToClose = new();

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
                            Exceptions.Ignore(() => o.Remove(connection.StreamId));
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

        public void Disconnect(Guid streamId)
        {
            _activeConnections.Use((o) =>
            {
                if (o.TryGetValue(streamId, out var activeEndpointConnection))
                {
                    Exceptions.Ignore(activeEndpointConnection.Disconnect);
                    Exceptions.Ignore(activeEndpointConnection.Dispose);
                    o.Remove(streamId);
                }
            });
        }

        public void SendEndpointData(Guid streamId, byte[] buffer)
        {
            lock (_statisticsLock)
            {
                BytesSent += (ulong)buffer.Length;
                _tunnel.BytesReceived += (ulong)buffer.Length;
            }

            var outboundConnection = _activeConnections.Use((o) =>
            {
                if (o.TryGetValue(streamId, out var outboundConnection))
                {
                    return outboundConnection;
                }

                return outboundConnection;
            });

            outboundConnection?.Write(buffer);
        }

        /// <summary>
        /// This is where data is received by the endpoint client connections (e.g. web-browser, web-server, etc).
        /// </summary>
        /// <param name="obj"></param>
        internal void EndpointDataExchangeThreadProc(object? obj)
        {
            Thread.CurrentThread.Name = $"EndpointDataExchangeThreadProc:{Environment.CurrentManagedThreadId}";

            var activeConnection = ((ActiveEndpointConnection?)obj).EnsureNotNull();

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
                    _tunnel.SendNotificationOfEndpointConnect(_tunnel.TunnelKey, EndpointId, activeConnection.StreamId);
                }

                var httpHeaderBuilder = new StringBuilder();

                var buffer = new PumpBuffer(Singletons.Configuration.InitialReceiveBufferSize);

                while (KeepRunning && activeConnection.IsConnected && activeConnection.Read(ref buffer))
                {
                    lock (_statisticsLock)
                    {
                        BytesReceived += (ulong)buffer.Length;
                        _tunnel.BytesReceived += (ulong)buffer.Length;
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
                                .Where(o => o.Enabled && (new[] { NtHttpHeaderType.Request, NtHttpHeaderType.Any }).Contains(o.HeaderType)).Any()
                            )
                            ||
                            (
                                // or the direction is outbound/any and we have response rules.
                                this is EndpointOutbound && Configuration.HttpHeaderRules
                                    .Where(o => o.Enabled && (new[] { NtHttpHeaderType.Request, NtHttpHeaderType.Any }).Contains(o.HeaderType)).Any()
                            )
                         )
                     )
                    {
                        switch (HttpUtility.Process(ref httpHeaderBuilder, Configuration, buffer))
                        {
                            case HttpUtility.HTTPHeaderResult.WaitOnData:
                                //We received a partial HTTP header, wait on more data.
                                continue;
                            case HttpUtility.HTTPHeaderResult.Present:
                                //Found an HTTP header, send it to the peer. There may be bytes remaining
                                //  in the buffer if buffer.Length > 0 so follow though to WriteBytesToPeer.

                                var httpHeaderBytes = Encoding.UTF8.GetBytes(httpHeaderBuilder.ToString());

                                //_tunnel.Notify(new oldNotificationEndpointExchange
                                //    (_tunnel.TunnelId, EndpointId, activeConnection.StreamId, httpHeaderBytes, httpHeaderBytes.Length));

                                httpHeaderBuilder.Clear();
                                break;
                            case HttpUtility.HTTPHeaderResult.NotPresent:
                                //Didn't find an HTTP header.
                                break;
                        }
                    }

                    #endregion

                    _tunnel.SendNotificationOfEndpointDataExchange(_tunnel.TunnelKey,
                        Configuration.EndpointId, activeConnection.StreamId, buffer.Bytes, buffer.Length);

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
                        _tunnel.ServiceEngine.Logger.Verbose($"EndpointDataExchangeThreadProc: {ex.Message}");
                    }
                    else
                    {
                        _tunnel.ServiceEngine.Logger.Exception($"EndpointDataExchangeThreadProc: {ex.Message}");
                    }
                }
                else
                {
                    _tunnel.ServiceEngine.Logger.Exception($"EndpointDataExchangeThreadProc: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _tunnel.ServiceEngine.Logger.Exception($"EndpointDataExchangeThreadProc: {ex.Message}");
            }
            finally
            {
                lock (_statisticsLock)
                {
                    CurrentConnections--;
                }

                Exceptions.Ignore(activeConnection.Disconnect);

                _activeConnections.Use((o) =>
                {
                    o.Remove(activeConnection.StreamId);
                });
            }

            //Exceptions.Ignore(() =>
            //    _tunnel.Notify(new oldNotificationEndpointDisconnect(_tunnel.TunnelId, EndpointId, activeConnection.StreamId)));
        }
    }
}
