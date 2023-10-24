using NetTunnel.Library;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.Types;
using NTDLS.Semaphore;
using static NetTunnel.Service.PacketFraming.Types;

namespace NetTunnel.Service.Engine
{
    internal class BaseEndpoint
    {
        public Guid PairId { get; private set; }
        public string Name { get; private set; }

        internal readonly EngineCore _core;
        internal readonly ITunnel _tunnel;
        internal bool _keepRunning = false;

        internal readonly CriticalResource<Dictionary<Guid, ActiveEndpointConnection>> _activeConnections = new();

        public BaseEndpoint(EngineCore core, ITunnel tunnel, Guid pairId, string name)
        {
            _core = core;
            _tunnel = tunnel;
            Name = name;
            PairId = pairId;
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

        internal void SendEndpointData(Guid streamId, byte[] buffer)
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
                _tunnel.SendStreamPacketNotification(new NtPacketPayloadEndpointConnect(_tunnel.PairId, PairId, activeConnection.StreamId));

                while (_keepRunning && activeConnection.IsConnected)
                {
                    byte[] buffer = new byte[NtPacketDefaults.PACKET_BUFFER_SIZE];
                    while (activeConnection.Read(ref buffer))
                    {
                        var exchnagePayload = new NtPacketPayloadEndpointExchange(_tunnel.PairId, PairId, activeConnection.StreamId, buffer);
                        _tunnel.SendStreamPacketNotification(exchnagePayload);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                activeConnection.Disconnect();
            }

            try
            {
                _tunnel.SendStreamPacketNotification(new NtPacketPayloadEndpointDisconnect(_tunnel.PairId, PairId, activeConnection.StreamId));
            }
            catch { }
        }
    }
}
