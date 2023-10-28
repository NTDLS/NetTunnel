using NetTunnel.Library;
using NetTunnel.Service.MessageFraming.FramePayloads.Notifications;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.Semaphore;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    internal class BaseEndpoint : IEndpoint
    {
        public int TransmissionPort { get; private set; }
        public ulong BytesReceived { get; internal set; }
        public ulong BytesSent { get; internal set; }
        public ulong TotalConnections { get; internal set; }
        public ulong CurrentConnections { get; internal set; }
        public Guid PairId { get; private set; }
        public string Name { get; private set; }

        internal readonly TunnelEngineCore _core;
        internal readonly ITunnel _tunnel;
        public bool KeepRunning { get; internal set; } = false;

        private readonly Thread _heartbeatThread;

        internal readonly CriticalResource<Dictionary<Guid, ActiveEndpointConnection>> _activeConnections = new();

        public BaseEndpoint(TunnelEngineCore core, ITunnel tunnel, Guid pairId, string name, int transmissionPort)
        {
            _core = core;
            _tunnel = tunnel;
            Name = name;
            PairId = pairId;
            TransmissionPort = transmissionPort;

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        private void HeartbeatThreadProc()
        {
            DateTime lastheartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastheartBeat).TotalMilliseconds > Singletons.Configuration.HeartbeatDelayMs)
                {
                    _activeConnections.Use((o) =>
                    {
                        List<ActiveEndpointConnection> connectionsToClose = new();

                        foreach (var connection in o)
                        {
                            Utility.TryAndIgnore(() =>
                            {
                                //We've are connected but havent done much in a while.
                                if (connection.Value.ActivityAgeInMiliseconds > Singletons.Configuration.MaxStaleConnectionAgeMs)
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

                    lastheartBeat = DateTime.UtcNow;
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

        internal void HandleClientThreadProc(object? obj)
        {
            Utility.EnsureNotNull(obj);
            var activeConnection = (ActiveEndpointConnection)obj;

            try
            {
                TotalConnections++;
                CurrentConnections++;

                _tunnel.SendStreamFrameNotification(new NtFramePayloadEndpointConnect(_tunnel.PairId, PairId, activeConnection.StreamId));

                byte[] buffer = new byte[Singletons.Configuration.FramebufferSize];
                while (KeepRunning && activeConnection.IsConnected)
                {
                    while (activeConnection.Read(ref buffer, out int length))
                    {
                        BytesReceived += (ulong)length;

                        var exchnagePayload = new NtFramePayloadEndpointExchange(_tunnel.PairId, PairId, activeConnection.StreamId, buffer, length);
                        _tunnel.SendStreamFrameNotification(exchnagePayload);
                    }
                }
            }
            catch (Exception ex)
            {
                _tunnel.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"HandleClientThreadProc: {ex.Message}");
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
                _tunnel.SendStreamFrameNotification(new NtFramePayloadEndpointDisconnect(_tunnel.PairId, PairId, activeConnection.StreamId)));
        }
    }
}
