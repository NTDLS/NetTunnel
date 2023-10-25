﻿using NetTunnel.Library.Types;
using NetTunnel.Service.Types;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening tunnel.
    /// </summary>
    internal class TunnelOutbound : BaseTunnel, ITunnel
    {
        private Thread? _outboundConnectionThread;
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public TunnelOutbound(EngineCore core, NtTunnelOutboundConfiguration configuration)
            : base(core, configuration)
        {
            Address = configuration.Address;
            ManagementPort = configuration.ManagementPort;
            DataPort = configuration.DataPort;
            Username = configuration.Username;
            PasswordHash = configuration.PasswordHash;
        }

        public NtTunnelOutboundConfiguration CloneConfiguration()
        {
            var tunnelConfiguration = new NtTunnelOutboundConfiguration(PairId, Name, Address, ManagementPort, DataPort, Username, PasswordHash);

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

        public override void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }

            Core.Logging.Write($"Starting outbound tunnel '{Name}'.");
            base.Start();

            _outboundConnectionThread = new Thread(OutboundConnectionThreadProc);
            _outboundConnectionThread.Start();

            Core.Logging.Write($"Starting inbound endpoints for outbound tunnel '{Name}'.");
            _inboundEndpoints.ForEach(x => x.Start());

            Core.Logging.Write($"Starting outbound endpoints for outbound tunnel '{Name}'.");
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public override void Stop()
        {
            Core.Logging.Write($"Stopping outbound tunnel '{Name}'.");
            base.Stop();
            _outboundConnectionThread?.Join(); //Wait on thread to finish.
            Core.Logging.Write($"Stopped outbound tunnel '{Name}'.");
        }

        private void OutboundConnectionThreadProc()
        {
            while (KeepRunning)
            {
                try
                {
                    Core.Logging.Write($"Outbound tunnel '{Name}' connecting to remote at {Address}:{DataPort}.");

                    var tcpClient = new TcpClient(Address, DataPort);

                    Core.Logging.Write($"Outbound tunnel '{Name}' connection successful.");

                    using (Stream = tcpClient.GetStream())
                    {
                        ReceiveAndProcessStreamFrames(ProcessFrameNotificationCallback, ProcessFrameQueryCallback);
                    }

                    Core.Logging.Write($"Outbound tunnel '{Name}' disconnected.");

                    tcpClient.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception[OutboundConnectionThreadProc]: {ex.Message}");
                }
            }
        }
    }
}
