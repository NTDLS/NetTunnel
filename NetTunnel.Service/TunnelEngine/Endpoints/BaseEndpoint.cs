using NetTunnel.Library;
using NetTunnel.Service.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.Semaphore;
using System.Net.Sockets;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    internal class BaseEndpoint : IEndpoint
    {
        public int TransmissionPort { get; private set; }
        public ulong BytesReceived { get; internal set; }
        public ulong BytesSent { get; internal set; }
        public ulong TotalConnections { get; internal set; }
        public ulong CurrentConnections { get; internal set; }
        public Guid EndpointId { get; private set; }
        public string Name { get; private set; }

        internal readonly TunnelEngineCore _core;
        internal readonly ITunnel _tunnel;
        public bool KeepRunning { get; internal set; } = false;

        private readonly Thread _heartbeatThread;

        internal readonly PessimisticCriticalResource<Dictionary<Guid, ActiveEndpointConnection>> _activeConnections = new();

        public BaseEndpoint(TunnelEngineCore core, ITunnel tunnel, Guid endpointId, string name, int transmissionPort)
        {
            _core = core;
            _tunnel = tunnel;
            Name = name;
            EndpointId = endpointId;
            TransmissionPort = transmissionPort;

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
                            Utility.TryAndIgnore(() =>
                            {
                                //We've are connected but haven't done much in a while.
                                if (connection.Value.ActivityAgeInMilliseconds > Singletons.Configuration.StaleEndpointExpirationMs)
                                {
                                    connectionsToClose.Add(connection.Value);
                                }
                            });
                        }

                        foreach (var connection in connectionsToClose)
                        {
                            Utility.TryAndIgnore(connection.Disconnect);
                            Utility.TryAndIgnore(() => o.Remove(connection.StreamId));
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
                    Utility.TryAndIgnore(activeEndpointConnection.Disconnect);
                    Utility.TryAndIgnore(activeEndpointConnection.Dispose);
                    o.Remove(streamId);
                }
            });
        }

        public void SendEndpointData(Guid streamId, byte[] buffer)
        {
            BytesSent += (ulong)buffer.Length;

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

        internal void EndpointDataExchangeThreadProc(object? obj)
        {
            Thread.CurrentThread.Name = $"EndpointDataExchangeThreadProc:{Environment.CurrentManagedThreadId}";

            var activeConnection = ((ActiveEndpointConnection?)obj).EnsureNotNull();

            try
            {
                TotalConnections++;
                CurrentConnections++;

                if (this is EndpointInbound)
                {
                    //If this is an inbound endpoint, then let the remote service know that we just received a
                    //  connection so that it came make an associated outbound connection.
                    _tunnel.Notify(new NotificationEndpointConnect(_tunnel.TunnelId, EndpointId, activeConnection.StreamId));
                }

                var buffer = new byte[Singletons.Configuration.EndpointBufferSize];
                while (KeepRunning && activeConnection.IsConnected && activeConnection.Read(ref buffer, out int length))
                {
                    BytesReceived += (ulong)length;

                    var exchangePayload = new NotificationEndpointExchange(_tunnel.TunnelId, EndpointId, activeConnection.StreamId, buffer, length);
                    _tunnel.Notify(exchangePayload);
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException sockEx)
                {
                    if (sockEx.SocketErrorCode == SocketError.ConnectionAborted)
                    {
                        //We don't typically care about this. This is something as simple as a user closing a web-browser.
                        _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Verbose, $"EndpointDataExchangeThreadProc: {ex.Message}");
                    }
                    else
                    {
                        _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"EndpointDataExchangeThreadProc: {ex.Message}");
                    }
                }
                else
                {
                    _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"EndpointDataExchangeThreadProc: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"EndpointDataExchangeThreadProc: {ex.Message}");
            }
            finally
            {
                CurrentConnections--;

                Utility.TryAndIgnore(activeConnection.Disconnect);

                _activeConnections.Use((o) =>
                {
                    o.Remove(activeConnection.StreamId);
                });
            }

            Utility.TryAndIgnore(() =>
                _tunnel.Notify(new NotificationEndpointDisconnect(_tunnel.TunnelId, EndpointId, activeConnection.StreamId)));
        }
    }
}
