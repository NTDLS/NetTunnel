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
                    activeEndpointConnection.Dispose();

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

            outboundConnection?.Stream.Write(buffer);
        }

        internal void HandleClientThreadProc(object? obj)
        {
            Utility.EnsureNotNull(obj);
            var param = (ActiveEndpointConnection)obj;

            try
            {
                _tunnel.SendStreamPacketNotification(new NtPacketPayloadEndpointConnect(_tunnel.PairId, PairId, param.StreamId));

                while (_keepRunning)
                {
                    byte[] buffer = new byte[NtPacketDefaults.PACKET_BUFFER_SIZE];
                    int bytesRead;
                    while ((bytesRead = param.Stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var exchnagePayload = new NtPacketPayloadEndpointExchange(_tunnel.PairId, PairId, param.StreamId, buffer);
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
                param.TcpClient.Close();
            }

            _tunnel.SendStreamPacketNotification(new NtPacketPayloadEndpointDisconnect(_tunnel.PairId, PairId, param.StreamId));
        }
    }
}
