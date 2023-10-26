using NetTunnel.Library;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.Types;
using NTDLS.Semaphore;
using static NetTunnel.Service.PacketFraming.Types;

namespace NetTunnel.Service.Engine
{
    internal class BaseEndpoint : IEndpoint
    {
        public Guid PairId { get; private set; }
        public string Name { get; private set; }

        internal readonly EngineCore _core;
        internal readonly ITunnel _tunnel;
        public bool KeepRunning { get; internal set; } = false;

        private readonly Thread _heartbeatThread;

        internal readonly CriticalResource<Dictionary<Guid, ActiveEndpointConnection>> _activeConnections = new();

        public BaseEndpoint(EngineCore core, ITunnel tunnel, Guid pairId, string name)
        {
            _core = core;
            _tunnel = tunnel;
            Name = name;
            PairId = pairId;

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        private void HeartbeatThreadProc()
        {
            DateTime lastheartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastheartBeat).TotalMilliseconds > 10000)
                {
                    _activeConnections.Use((o) =>
                    {
                        List<ActiveEndpointConnection> connectionsToClose = new();

                        foreach (var connection in o)
                        {
                            Utility.TryAndIgnore(() =>
                            {
                                //We've are connected but havent done much in a while.
                                if (connection.Value.ActivityAgeInMiliseconds > 600 * 1000)
                                {
                                    connectionsToClose.Add(connection.Value);
                                }
                            });

                            Utility.TryAndIgnore(() =>
                            {
                                //We've been in the queue for more than 10 seconds and we still arent connected.
                                if (connection.Value.StartAgeInMiliseconds > 10 * 1000 && connection.Value.IsConnected == false)
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
                    try
                    {
                        activeEndpointConnection.Disconnect();
                        activeEndpointConnection.Dispose();
                    }
                    catch { }

                    o.Remove(streamId);
                }
            });
        }

        public void SendEndpointData(Guid streamId, byte[] buffer)
        {
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
                _tunnel.SendStreamFrameNotification(new NtFramePayloadEndpointConnect(_tunnel.PairId, PairId, activeConnection.StreamId));

                while (KeepRunning && activeConnection.IsConnected)
                {
                    byte[] buffer = new byte[NtFrameDefaults.FRAME_BUFFER_SIZE];
                    while (activeConnection.Read(ref buffer))
                    {
                        var exchnagePayload = new NtFramePayloadEndpointExchange(_tunnel.PairId, PairId, activeConnection.StreamId, buffer);
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
                Utility.TryAndIgnore(activeConnection.Disconnect);

                _activeConnections.Use((o) =>
                {
                    o.Remove(activeConnection.StreamId);
                });
            }

            try
            {
                _tunnel.SendStreamFrameNotification(new NtFramePayloadEndpointDisconnect(_tunnel.PairId, PairId, activeConnection.StreamId));
            }
            catch { }
        }
    }
}
