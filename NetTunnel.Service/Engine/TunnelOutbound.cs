﻿using NetTunnel.Library.Types;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening tunnel.
    /// </summary>
    public class TunnelOutbound
    {
        private readonly EngineCore _core;
        private NtTunnelOutboundConfiguration _configuration;
        private Thread? _outgoingConnectionThread;
        private bool _keepRunning = false;

        private List<EndpointInbound> _inboundEndpoints = new();
        private List<EndpointOutbound> _outboundEndpoints = new();

        public Guid Id { get => _configuration.Id; }

        public TunnelOutbound(EngineCore core, NtTunnelOutboundConfiguration config)
        {
            _core = core;
            _configuration = config;

            foreach (var cfg in config.InboundEndpointConfigurations)
            {
                _inboundEndpoints.Add(new(_core, cfg));
            }

            foreach (var cfg in config.OutboundEndpointConfigurations)
            {
                _outboundEndpoints.Add(new(_core, cfg));
            }
        }

        public NtTunnelOutboundConfiguration CloneConfiguration() => _configuration.Clone();

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing tunnel '{_configuration.Name}'");

            _outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            _outgoingConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
        }

        void OutgoingConnectionThreadProc()
        {
            while (_keepRunning)
            {
                try
                {
                    _core.Logging.Write($"Attempting to connect to outgoing tunnel '{_configuration.Name}' at {_configuration.Address}:{_configuration.DataPort}.");

                    var client = new TcpClient(_configuration.Address, _configuration.DataPort);

                    _core.Logging.Write($"Connection successful for tunnel '{_configuration.Name}' at {_configuration.Address}:{_configuration.DataPort}.");

                    HandleClient(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

        }

        void HandleClient(TcpClient client)
        {
            while (_keepRunning)
            {
                /*
                using (NetworkStream stream = client.GetStream())
                {
                    // Send data to the server.
                    string message = "Hello, server!";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // Receive data from the server.
                    byte[] receiveBuffer = new byte[1024];
                    int bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                    string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                    Console.WriteLine($"Received: {receivedMessage}");
                }
                */
                Thread.Sleep(10);
            }

            client.Close();
        }

    }
}