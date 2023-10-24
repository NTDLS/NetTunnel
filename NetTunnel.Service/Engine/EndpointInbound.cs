using NetTunnel.ClientAPI;
using NetTunnel.Library;
using NetTunnel.Library.Types;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.Types;
using NTDLS.Semaphore;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using static NetTunnel.Service.PacketFraming.Types;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class EndpointInbound
    {
        private readonly EngineCore _core;
        private Thread? _incomingConnectionThread;
        private bool _keepRunning = false;
        private readonly ITunnel _tunnel;

        public Guid PairId { get; private set; }
        public string Name { get; private set; }
        public int Port { get; private set; }

        public EndpointInbound(EngineCore core, ITunnel tunnel, NtEndpointInboundConfiguration configuration)
        {
            _core = core;
            _tunnel = tunnel;

            PairId = configuration.PairId;
            Name = configuration.Name;
            Port = configuration.Port;
        }

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting incoming endpoint '{Name}' on port {Port}");

            _incomingConnectionThread = new Thread(IncomingConnectionThreadProc);
            _incomingConnectionThread.Start();
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        void IncomingConnectionThreadProc()
        {
            var tcpListener = new TcpListener(IPAddress.Any, Port);

            Guid streamId = Guid.NewGuid();

            try
            {
                tcpListener.Start();

                _core.Logging.Write($"Listening incoming endpoint '{Name}' on port {Port}");

                while (_keepRunning)
                {
                    _core.Logging.Write($"Waiting for connection for incoming endpoint '{Name}' on port {Port}");

                    var tcpClient = tcpListener.AcceptTcpClient();

                    _core.Logging.Write($"Accepted on incoming endpoint '{Name}' on port {Port}");


                    var handlerThread = new Thread(HandleClientThreadProc);

                    var param = new OutboundConnection(handlerThread, tcpClient, streamId);

                    handlerThread.Start(param);

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Stop listening and close the listener when done.
                tcpListener.Stop();
            }
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

                return outboundConnection;
            });

            outboundConnection.Stream.Write(buffer);
        }

        private void HandleClientThreadProc(object? obj)
        {
            Utility.EnsureNotNull(obj);
            var param = (OutboundConnection)obj;

            using (var tcpStream = param.TcpClient.GetStream())
            {
                //Here we need to tell the remote service to start an outgoing connection for this endpoint and on its owning tunnel.

                while (_keepRunning)
                {
                    byte[] buffer = new byte[NtPacketDefaults.PACKET_BUFFER_SIZE];
                    int bytesRead;
                    while ((bytesRead = tcpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var exchnagePayload = new NtPacketPayloadEndpointExchange(_tunnel.PairId, PairId, param.StreamId, buffer);
                        _tunnel.SendStreamPacketNotification(exchnagePayload);
                    }

                    Thread.Sleep(1);
                }
            }

            param.TcpClient.Close();
        }
    }
}
