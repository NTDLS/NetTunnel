﻿using NetTunnel.Library.Types;
using System.Net.Sockets;

namespace NetTunnel.Service.Engine
{
    /// <summary>
    /// This is the class that makes an outgoing TCP/IP connection to a listening tunnel.
    /// </summary>
    public class TunnelOutbound : ITunnel
    {
        private readonly EngineCore _core;
        //private readonly NtTunnelOutboundConfiguration _configuration;
        private Thread? _outgoingConnectionThread;
        private bool _keepRunning = false;

        private readonly List<EndpointInbound> _inboundEndpoints = new();
        private readonly List<EndpointOutbound> _outboundEndpoints = new();

        public Guid PairId { get; private set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int ManagementPort { get; set; }
        public int DataPort { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public TunnelOutbound(EngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            _core = core;

            PairId = configuration.PairId;
            Name = configuration.Name;
            Address = configuration.Address;
            ManagementPort = configuration.ManagementPort;
            DataPort = configuration.DataPort;
            Username = configuration.Username;
            PasswordHash = configuration.PasswordHash;

            configuration.InboundEndpointConfigurations.ForEach(o => _inboundEndpoints.Add(new(_core, this, o)));
            configuration.OutboundEndpointConfigurations.ForEach(o => _outboundEndpoints.Add(new(_core, this, o)));
        }

        public void DispatchMessage(string message)
        {
            throw new NotImplementedException();
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

        public void AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
            => _inboundEndpoints.Add(new EndpointInbound(_core, this, configuration));

        public void AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
            => _outboundEndpoints.Add(new EndpointOutbound(_core, this, configuration));

        public void DeleteInboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
        }

        public void DeleteOutboundEndpoint(Guid endpointPairId)
        {
            var endpoint = _inboundEndpoints.Where(o => o.PairId == endpointPairId).Single();
            endpoint.Stop();
            _inboundEndpoints.Remove(endpoint);
        }

        public void Start()
        {
            _keepRunning = true;

            _core.Logging.Write($"Starting outgoing tunnel '{Name}'");

            _outgoingConnectionThread = new Thread(OutgoingConnectionThreadProc);
            _outgoingConnectionThread.Start();

            _inboundEndpoints.ForEach(x => x.Start());
            _outboundEndpoints.ForEach(x => x.Start());
        }

        public void Stop()
        {
            _keepRunning = false;
            //TODO: Wait on thread(s) to stop.
        }

        private void OutgoingConnectionThreadProc()
        {
            while (_keepRunning)
            {
                try
                {
                    _core.Logging.Write($"Attempting to connect to outgoing tunnel '{Name}' at {Address}:{DataPort}.");

                    var client = new TcpClient(Address, DataPort);

                    _core.Logging.Write($"Connection successful for tunnel '{Name}' at {Address}:{DataPort}.");

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
