using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.Types;
using NTDLS.Semaphore;
using System;
using System.Linq.Expressions;
using System.Net.Sockets;
using static NetTunnel.Service.PacketFraming.Types;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening endpoint.
    /// </summary>
    internal class EndpointOutbound : IEndpoint
    {
        private readonly EngineCore _core;
        private bool _keepRunning = false;
        private readonly ITunnel _tunnel;

        public Guid PairId { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }

        public EndpointOutbound(EngineCore core, ITunnel tunnel, NtEndpointOutboundConfiguration configuration)
        {
            _core = core;
            _tunnel = tunnel;

            PairId = configuration.PairId;
            Name = configuration.Name;
            Address = configuration.Address;
            Port = configuration.Port;
        }

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing endpoint '{Name}'");

            //_outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            //_outgoingConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private class OutboundConnection : IDisposable
        {
            public Guid StreamId { get; set; }
            public TcpClient TcpClient { get; set; }
            public Thread Thread { get; set; }
            public NetworkStream Stream { get; set; }

            public OutboundConnection(Thread thread, TcpClient tcpClient, Guid streamId)
            {
                Thread = thread;
                TcpClient = tcpClient;
                StreamId = streamId;
                Stream = tcpClient.GetStream();
            }

            public void Dispose()
            {
                Stream.Dispose();
            }
        }

        CriticalResource<Dictionary<Guid, OutboundConnection>> _activeConnections = new();

        public void SendEndpointData(Guid streamId, byte[] buffer)
        {
            var outboundConnection = _activeConnections.Use((o) =>
            {
                if (o.TryGetValue(streamId, out var outboundConnection))
                {
                    return outboundConnection;
                }

                outboundConnection = StartConnection(streamId);

                o.Add(streamId, outboundConnection);

                return outboundConnection;
            });

            outboundConnection.Stream.Write(buffer);
        }

        private OutboundConnection StartConnection(Guid streamId)
        {
            var tcpClient = new TcpClient();

            try
            {
                _core.Logging.Write($"Connecting outbound endpoint '{Name}' on port {Port}"); ;

                tcpClient.Connect(Address, Port);

                _core.Logging.Write($"Accepted on incoming endpoint '{Name}' on port {Port}");

                var handlerThread = new Thread(HandleClientThreadProc);
                var param = new OutboundConnection(handlerThread, tcpClient, streamId);

                handlerThread.Start(param);

                return param;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        private void HandleClientThreadProc(object? obj)
        {
            Utility.EnsureNotNull(obj);
            var param = (OutboundConnection)obj;

            try
            {
                //Here we need to tell the remote service to start an outgoing connection for this endpoint and on its owning tunnel.

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
            catch
            {
                throw;
            }
            finally
            {
                param.TcpClient.Close();
            }
        }
    }
}
