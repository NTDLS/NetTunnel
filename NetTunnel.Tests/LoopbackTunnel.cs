using System.Net;
using System.Net.Sockets;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Query.ServiceToService;
using NetTunnel.Service.TunnelEngine.Endpoints;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// One side of a tunnel, joined directly to the other side in the same process. It stands in for the messaging layer the way it
    /// really behaves: connecting is a call that waits for the other side, while data and disconnects are notifications which are handled
    /// concurrently, after a random delay, and so not in the order they were sent.
    /// </summary>
    internal sealed class LoopbackTunnelSide(NtDirection direction, Guid tunnelId) : ITunnel
    {
        private static readonly Random Jitter = new(4711);

        public LoopbackTunnelSide? Peer { get; set; }
        public Dictionary<Guid, BaseEndpoint> EndpointsById { get; } = [];

        /// <summary>
        /// Send the old kind of disconnect, which says nothing about how many packets precede it.
        /// </summary>
        public bool LegacyDisconnect { get; set; }

        /// <summary>
        /// Handle notifications one after the other, as soon as they are sent, instead of concurrently and out of order.
        /// </summary>
        public bool DeliverInOrder { get; set; }

        public int ConnectsSent;
        public int DataPacketsSent;

        public NtDirection Direction => direction;
        public DirectionalKey TunnelKey => new(tunnelId, direction);
        public bool IsLoggedIn => true;

        private static int NextDelayMs()
        {
            lock (Jitter)
            {
                return Jitter.Next(0, 4);
            }
        }

        private void Notify(Action handle)
        {
            if (DeliverInOrder)
            {
                handle();
                return;
            }

            Task.Run(async () =>
            {
                await Task.Delay(NextDelayMs());
                handle();
            });
        }

        public void S2SPeerNotificationEndpointConnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, string? applicationProtocol = null)
        {
            Interlocked.Increment(ref ConnectsSent);
            ((EndpointOutbound)Peer!.EndpointsById[endpointId]).EstablishOutboundEndpointConnection(edgeId, applicationProtocol); //A query: waits.
        }

        public void S2SPeerNotificationEndpointDataExchange(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, long sequence, byte[] bytes, int length)
        {
            Interlocked.Increment(ref DataPacketsSent);
            var copy = bytes.AsSpan(0, length).ToArray(); //The real payload copies the bytes too.
            var peer = Peer!;
            Notify(() => peer.EndpointsById[endpointId].WriteEndpointEdgeData(edgeId, sequence, copy));
        }

        public void S2SPeerNotificationEndpointDisconnect(DirectionalKey tunnelKey, Guid endpointId, Guid edgeId, long packetCount = -1)
        {
            var peer = Peer!;
            var count = LegacyDisconnect ? -1 : packetCount;
            Notify(() => peer.EndpointsById[endpointId].Disconnect(edgeId, count));
        }

        #region Not needed by the endpoints.

        public double? PingTime { get; set; }
        public TunnelConfiguration Configuration => throw new NotSupportedException();
        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; set; }
        public ulong CurrentConnections { get; set; }
        public IServiceEngine ServiceEngine => throw new NotSupportedException();
        public IReadOnlyList<IEndpoint> Endpoints => throw new NotSupportedException();
        public void Start() { }
        public void Stop() { }
        public IEndpoint UpsertEndpoint(EndpointConfiguration configuration, string? username) => throw new NotSupportedException();
        public void DeleteEndpoint(Guid endpointId, string? username) => throw new NotSupportedException();
        public void DisconnectEndpointEdge(Guid endpointId, Guid edgeId, long packetCount = -1) => throw new NotSupportedException();
        public TunnelConfiguration CloneConfiguration() => throw new NotSupportedException();
        public void S2SPeerNotificationTunnelDeletion(DirectionalKey tunnelKey) => throw new NotSupportedException();
        public void S2SPeerNotificationEndpointDeletion(DirectionalKey tunnelKey, Guid endpointId) => throw new NotSupportedException();
        public S2SQueryUpsertEndpointReply S2SPeerQueryUpsertEndpoint(DirectionalKey tunnelKey, EndpointConfiguration endpointId) => throw new NotSupportedException();
        public void LoadEndpoints(List<EndpointConfiguration> endpoints) => throw new NotSupportedException();
        public void IncrementBytesSent(int bytes) { }
        public void IncrementBytesReceived(int bytes) { }
        public TunnelPropertiesDisplay GetProperties() => throw new NotSupportedException();
        public EndpointPropertiesDisplay GetEndpointProperties(DirectionalKey endpointKey) => throw new NotSupportedException();
        public List<EndpointEdgeConnectionDisplay> GetEndpointEdgeConnections(DirectionalKey endpointKey) => throw new NotSupportedException();
        public List<EndpointConfiguration> GetEndpointsForDisplay() => throw new NotSupportedException();

        #endregion
    }

    /// <summary>
    /// An inbound endpoint on one side and the matching outbound endpoint on the other, joined as a tunnel is, in front of a server.
    /// </summary>
    internal sealed class LoopbackTunnel : IDisposable
    {
        public LoopbackTunnelSide InboundSide { get; }
        public LoopbackTunnelSide OutboundSide { get; }
        public EndpointInbound Inbound { get; }
        public EndpointOutbound Outbound { get; }
        public int ListenPort { get; }

        public LoopbackTunnel(int serverPort, Action<EndpointConfiguration>? configure = null, bool start = true)
        {
            var tunnelId = Guid.NewGuid();
            InboundSide = new LoopbackTunnelSide(NtDirection.Inbound, tunnelId);
            OutboundSide = new LoopbackTunnelSide(NtDirection.Outbound, tunnelId);
            InboundSide.Peer = OutboundSide;
            OutboundSide.Peer = InboundSide;

            ListenPort = FreePort();

            var configuration = new EndpointConfiguration
            {
                EndpointId = Guid.NewGuid(),
                Name = "test",
                InboundPort = ListenPort,
                OutboundPort = serverPort,
                OutboundAddress = "127.0.0.1",
                Direction = NtDirection.Inbound
            };
            configure?.Invoke(configuration);

            Inbound = new EndpointInbound(null!, InboundSide, configuration);
            Outbound = new EndpointOutbound(null!, OutboundSide, configuration.SwapDirection());

            InboundSide.EndpointsById[configuration.EndpointId] = Inbound;
            OutboundSide.EndpointsById[configuration.EndpointId] = Outbound;

            Outbound.Start();
            if (start)
            {
                Inbound.Start();
            }
        }

        public void Dispose()
        {
            try { Inbound.Stop(); } catch { /* best effort */ }
            try { Outbound.Stop(); } catch { /* best effort */ }
        }

        public static int FreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public TcpClient ConnectClient()
        {
            var client = new TcpClient();
            client.Connect(IPAddress.Loopback, ListenPort);
            return client;
        }
    }

    /// <summary>
    /// A TCP server that the outbound endpoint connects to.
    /// </summary>
    internal sealed class TestServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new();

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        /// <summary>Everything each accepted connection received before the client closed it.</summary>
        public System.Collections.Concurrent.ConcurrentBag<byte[]> Received { get; } = [];

        public TestServer(Func<TcpClient, CancellationToken, Task> handle)
        {
            _listener.Start();
            _ = Task.Run(async () =>
            {
                while (!_stop.IsCancellationRequested)
                {
                    TcpClient client;
                    try { client = await _listener.AcceptTcpClientAsync(_stop.Token); }
                    catch { return; }
                    _ = Task.Run(async () =>
                    {
                        try { await handle(client, _stop.Token); }
                        catch { /* the test decides what it expects */ }
                        finally { client.Dispose(); }
                    });
                }
            });
        }

        public static TestServer Echo() => new(async (client, token) =>
        {
            var stream = client.GetStream();
            var buffer = new byte[16384];
            int read;
            while ((read = await stream.ReadAsync(buffer, token)) > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, read), token);
            }
        });

        public void Dispose()
        {
            _stop.Cancel();
            _listener.Stop();
        }
    }
}
