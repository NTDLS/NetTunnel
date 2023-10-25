﻿using NetTunnel.Library.Types;
using NetTunnel.Service.Types;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that opens a listening TCP/IP port to wait on connections from a remote tunnel.
    /// </summary>
    internal class TunnelInbound : BaseTunnel, ITunnel
    {
        private Thread? _inboundConnectionThread;
        public int DataPort { get; private set; }

        public TunnelInbound(EngineCore core, NtTunnelInboundConfiguration configuration)
            : base(core, configuration)
        {
            DataPort = configuration.DataPort;
        }

        public NtTunnelInboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelInboundConfiguration(PairId, Name, DataPort);

            foreach (var endpoint in _inboundEndpoints)
            {
                var endpointConfiguration = new NtEndpointInboundConfiguration(endpoint.PairId, endpoint.Name, endpoint.Port);
                tunnelConfiguration.EndpointInboundConfigurations.Add(endpointConfiguration);
            }

            foreach (var endpoint in _outboundEndpoints)
            {
                var endpointConfiguration = new NtEndpointOutboundConfiguration(endpoint.PairId, endpoint.Name, endpoint.Address, endpoint.Port);
                tunnelConfiguration.EndpointOutboundConfigurations.Add(endpointConfiguration);
            }

            return tunnelConfiguration;
        }

        public void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }
            KeepRunning = true;

            Core.Logging.Write($"Starting inbound tunnel '{Name}' on port {DataPort}");

            _inboundConnectionThread = new Thread(InboundConnectionThreadProc);
            _inboundConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            KeepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private void InboundConnectionThreadProc()
        {
            var listener = new TcpListener(IPAddress.Any, DataPort);

            try
            {
                listener.Start();

                Core.Logging.Write($"Listening inbound tunnel '{Name}' on port {DataPort}");

                while (KeepRunning)
                {
                    Core.Logging.Write($"Waiting for connection for inbound tunnel '{Name}' on port {DataPort}");

                    var tcpClient = listener.AcceptTcpClient();
                    Core.Logging.Write($"Connected on inbound tunnel '{Name}' on port {DataPort}");

                    using (_stream = tcpClient.GetStream())
                    {
                        ReceiveAndProcessStreamFrames(ProcessFrameNotificationCallback, ProcessFrameQueryCallback);
                        try { _stream.Close(); } catch { }
                    }

                    try { tcpClient.Close(); } catch { }
                    try { tcpClient.Dispose(); } catch { }

                    Core.Logging.Write($"Disconnected inbound tunnel '{Name}' on port {DataPort}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Stop listening and close the listener when done.
                listener.Stop();
            }
        }
    }
}
