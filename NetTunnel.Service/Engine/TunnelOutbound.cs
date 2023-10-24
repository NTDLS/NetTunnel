﻿using NetTunnel.Library.Types;
using NetTunnel.Service.Packetizer.PacketPayloads;
using NetTunnel.Service.Types;
using System.Diagnostics;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening tunnel.
    /// </summary>
    internal class TunnelOutbound : BaseTunnel, ITunnel
    {
        private Thread? _outgoingConnectionThread;
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public TunnelOutbound(EngineCore core, NtTunnelOutboundConfiguration configuration)
            : base(core, configuration)
        {
            _core = core;

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
                tunnelConfiguration.InboundEndpointConfigurations.Add(endpointConfiguration);
            }

            foreach (var endpoint in _outboundEndpoints)
            {
                var endpointConfiguration = new NtEndpointOutboundConfiguration(endpoint.PairId, endpoint.Name, endpoint.Address, endpoint.Port);
                tunnelConfiguration.OutboundEndpointConfigurations.Add(endpointConfiguration);
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

            _core.Logging.Write($"Starting outgoing tunnel '{Name}'");

            _outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            _outgoingConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            KeepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private void OutgoingConnectionThreadProc()
        {
            while (KeepRunning)
            {
                try
                {
                    _core.Logging.Write($"Attempting to connect to outgoing tunnel '{Name}' at {Address}:{DataPort}.");

                    var tcpClient = new TcpClient(Address, DataPort);

                    _core.Logging.Write($"Connection successful for tunnel '{Name}' at {Address}:{DataPort}.");

                    using (_stream = tcpClient.GetStream())
                    {
                        ExecuteStream(ProcessPacketCallbackHandler);
                    }

                    tcpClient.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                Thread.Sleep(1);
            }
        }

        void ProcessPacketCallbackHandler(ITunnel tunnel, IPacketPayload packet)
        {
            if (packet is NtPacketPayloadMessage message)
            {
                Debug.Print($"{message.Message}");
            }
            else if (packet is NtPacketPayloadAddEndpointInbound inboundEndpoint)
            {
                AddInboundEndpoint(inboundEndpoint.Configuration);
                _core.OutboundTunnels.SaveToDisk();
            }
            else if (packet is NtPacketPayloadAddEndpointOutbound outboundEndpoint)
            {
                AddOutboundEndpoint(outboundEndpoint.Configuration);
                _core.OutboundTunnels.SaveToDisk();
            }
            else
            {

            }
        }
    }
}
