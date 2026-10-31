using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// The UDP counterpart of <see cref="EndpointInbound"/>: it receives the datagrams of clients on one port. Each client (address and
    /// port) gets a session, which makes the matching outbound endpoint open a socket to the destination; datagrams then travel through
    /// the tunnel as numbered packets, and replies come back the same way and are sent to the client from the port it wrote to.
    /// </summary>
    internal class UdpEndpointInbound : BaseEndpoint, IEndpoint
    {
        /// <summary>
        /// The datagrams of a session which are waiting to go into the tunnel. When a client sends faster than the tunnel carries,
        /// the ones that do not fit are dropped, as a congested network would drop them.
        /// </summary>
        private const int SessionQueueCapacity = 1024;

        /// <summary>
        /// The most bytes one session may have waiting. Counting datagrams is not enough: with up to 65,507 bytes each, and thousands of
        /// sessions, a few clients could otherwise use gigabytes of memory.
        /// </summary>
        private const int SessionQueueMaxBytes = 1024 * 1024;

        /// <summary>
        /// A client whose session could not be set up is ignored for this long, rather than costing a connect attempt per datagram.
        /// </summary>
        private static readonly TimeSpan RefusedFor = TimeSpan.FromSeconds(5);

        private sealed class SessionState(IPEndPoint key, UdpEdgeSession edge)
        {
            public IPEndPoint Key { get; } = key;
            public long QueuedBytes;
            public UdpEdgeSession Edge { get; } = edge;
            public Channel<byte[]> Queue { get; } = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(SessionQueueCapacity)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait //TryWrite fails when it is full, which is a dropped datagram.
            });
        }

        private readonly ConcurrentDictionary<IPEndPoint, SessionState> _sessions = new();
        private readonly ConcurrentDictionary<Guid, SessionState> _sessionsByEdge = new();
        private readonly ConcurrentDictionary<IPAddress, long> _refusedUntilTicks = new();
        private CancellationTokenSource? _stop;
        private Task? _receiveTask;
        private Socket? _socket;
        private long _lastLimitWarningTicks;

        /// <summary>
        /// True once the port is bound and datagrams are being received. The endpoint binds in the background after it is started, so a
        /// client that sends at once can be too early.
        /// </summary>
        public bool IsListening => _socket != null;

        public DirectionalKey EndpointKey => new(this);
        public NtDirection Direction => NtDirection.Inbound;
        public override int GetHashCode() => Configuration.GetHashCode();

        public UdpEndpointInbound(IServiceEngine serviceEngine, ITunnel tunnel, EndpointConfiguration configuration)
            : base(serviceEngine, tunnel, configuration.EndpointId, configuration)
        {
        }

        public override void Start()
        {
            if (TryBeginStart() == false)
            {
                return; //Already running.
            }

            Singletons.Logger.Verbose($"Starting inbound UDP endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            _stop = new CancellationTokenSource();
            var token = _stop.Token;
            _receiveTask = Task.Run(() => RunAsync(token));
        }

        public override void Stop()
        {
            if (TryBeginStop() == false)
            {
                return; //Not running.
            }

            Singletons.Logger.Verbose($"Stopping inbound UDP endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");

            Exceptions.Ignore(() => _stop?.Cancel());
            Exceptions.Ignore(() => _socket?.Dispose());

            foreach (var edge in _edgeConnections.Use(o => o.Values.ToList()))
            {
                Exceptions.Ignore(edge.Disconnect);
            }

            try
            {
                _receiveTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                //The loop ends with an exception when the socket is closed. That is how it is meant to end.
            }

            _receiveTask = null;
            _socket = null;

            Singletons.Logger.Verbose($"Stopped inbound UDP endpoint '{Configuration.Name}' on port {Configuration.InboundPort}.");
        }

        private async Task RunAsync(CancellationToken token)
        {
            Socket? socket = null;
            int bindAttempts = 0;

            //A port that is busy must not leave the endpoint "running" but deaf for ever, so keep trying until it is bound or the endpoint stops.
            while (socket == null && !token.IsCancellationRequested)
            {
                try
                {
                    socket = NetworkUtility.CreateUdpListenSocket(Configuration.InboundPort);
                }
                catch (SocketException ex) when (!token.IsCancellationRequested)
                {
                    if (bindAttempts++ % 12 == 0) //Once, then about once a minute.
                    {
                        Singletons.Logger.Warning($"Inbound UDP endpoint '{Configuration.Name}' cannot listen on port {Configuration.InboundPort}"
                            + $" and will keep trying: {ex.Message}");
                    }

                    try { await Task.Delay(TimeSpan.FromSeconds(5), token); }
                    catch (OperationCanceledException) { return; }
                }
            }

            if (socket == null)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                socket.Dispose();
                return;
            }

            _socket = socket;

            Singletons.Logger.Verbose($"Listening inbound UDP endpoint '{Configuration.Name}' on port {Configuration.InboundPort}"
                + $" ({(socket.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv4 and IPv6" : "IPv4")})");

            var buffer = new byte[UdpMaxDatagramSize];
            EndPoint any = socket.AddressFamily == AddressFamily.InterNetworkV6 ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);
            int failures = 0;

            while (!token.IsCancellationRequested)
            {
                SocketReceiveFromResult received;

                try
                {
                    received = await socket.ReceiveFromAsync(buffer, SocketFlags.None, any, token);
                    failures = 0;
                }
                catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
                {
                    break; //The endpoint was stopped.
                }
                catch (SocketException ex) when (token.IsCancellationRequested || ex.SocketErrorCode == SocketError.OperationAborted)
                {
                    break;
                }
                catch (SocketException ex) when (ex.SocketErrorCode is SocketError.ConnectionReset or SocketError.MessageSize or SocketError.NetworkReset)
                {
                    continue; //About a client that is gone, or a datagram that did not fit. Not about the socket.
                }
                catch (Exception ex)
                {
                    if (failures++ % 60 == 0)
                    {
                        Singletons.Logger.Warning($"Inbound UDP endpoint '{Configuration.Name}' failed to receive a datagram: {ex.Message}");
                    }

                    try { await Task.Delay(Math.Min(50 * failures, 2000), token); }
                    catch (OperationCanceledException) { break; }
                    continue;
                }

                try
                {
                    Dispatch((IPEndPoint)received.RemoteEndPoint, buffer, received.ReceivedBytes);
                }
                catch (Exception ex)
                {
                    Singletons.Logger.Exception(ex, $"Inbound UDP endpoint '{Configuration.Name}' failed to handle a datagram");
                }
            }
        }

        /// <summary>
        /// Gives a datagram to the session of its client, starting the session if the client has none.
        /// </summary>
        private void Dispatch(IPEndPoint client, byte[] buffer, int length)
        {
            if (length == 0)
            {
                return; //An empty datagram carries nothing to forward.
            }

            var key = new IPEndPoint(Utility.Unmap(client.Address), client.Port);

            if (_sessions.TryGetValue(key, out var existing))
            {
                Enqueue(existing, buffer, length);
                return;
            }

            if (_refusedUntilTicks.TryGetValue(key.Address, out var until))
            {
                if (DateTime.UtcNow.Ticks < until)
                {
                    return;
                }
                _refusedUntilTicks.TryRemove(key.Address, out _);
            }

            if (_sessions.Count >= Singletons.Configuration.UdpMaxSessionsPerEndpoint)
            {
                WarnAboutLimit();
                return;
            }

            var socket = _socket ?? throw new InvalidOperationException("The endpoint has no socket.");
            var edge = new UdpEdgeSession(Guid.NewGuid(), key, data => socket.SendTo(data, client), OnSessionClosed);
            var state = new SessionState(key, edge);

            _sessions[key] = state;
            _sessionsByEdge[edge.EdgeId] = state;
            _edgeConnections.Use(o => o[edge.EdgeId] = edge);
            NoteConnectionOpened();

            Singletons.Logger.Debug($"Accepted inbound UDP session {edge.EdgeId} from {key}");

            Enqueue(state, buffer, length);
            _ = Task.Run(() => RunSessionAsync(state));
        }

        private void Enqueue(SessionState state, byte[] buffer, int length)
        {
            var datagram = buffer.AsSpan(0, length).ToArray(); //The receive buffer is reused for the next datagram.

            state.Edge.NoteReceived(length);
            AddBytesReceived(length);

            if (Interlocked.Add(ref state.QueuedBytes, length) > SessionQueueMaxBytes || state.Queue.Writer.TryWrite(datagram) == false)
            {
                Interlocked.Add(ref state.QueuedBytes, -length);
                Singletons.Logger.Debug($"Inbound UDP session {state.Edge.EdgeId} dropped a datagram: the tunnel is not keeping up.");
            }
        }

        /// <summary>
        /// Tells the other end of the tunnel about the new session, then puts the datagrams of the session into the tunnel in the
        /// order they were received. This is not done by the receive loop: asking the other end can take a while, and the other clients
        /// must not wait for it.
        /// </summary>
        private async Task RunSessionAsync(SessionState state)
        {
            var edge = state.Edge;

            try
            {
                _tunnel.S2SPeerNotificationEndpointConnect(_tunnel.TunnelKey, EndpointId, edge.EdgeId, null);

                await foreach (var datagram in state.Queue.Reader.ReadAllAsync())
                {
                    Interlocked.Add(ref state.QueuedBytes, -datagram.Length);

                    if (!edge.IsConnected)
                    {
                        break;
                    }

                    _tunnel.S2SPeerNotificationEndpointDataExchange(
                        _tunnel.TunnelKey, EndpointId, edge.EdgeId, edge.NextSequence(), datagram, datagram.Length);
                }
            }
            catch (Exception ex)
            {
                Singletons.Logger.Warning($"Inbound UDP endpoint '{Configuration.Name}' could not start a session for {state.Key}: {ex.Message}");
                _refusedUntilTicks[state.Key.Address] = (DateTime.UtcNow + RefusedFor).Ticks;
                edge.Disconnect();
            }
        }

        /// <summary>
        /// The session ended, from either end: forget it and tell the peer how many packets it should have received.
        /// </summary>
        private void OnSessionClosed(UdpEdgeSession edge)
        {
            if (!_sessionsByEdge.TryRemove(edge.EdgeId, out var state))
            {
                return;
            }

            _sessions.TryRemove(new KeyValuePair<IPEndPoint, SessionState>(state.Key, state));
            state.Queue.Writer.TryComplete();
            _edgeConnections.Use(o => o.Remove(edge.EdgeId));
            NoteConnectionClosed();

            Singletons.Logger.Debug($"Ended inbound UDP session {edge.EdgeId} from {state.Key}");

            //Not on this thread: it may be one that holds a lock, and the notification goes over the network.
            long packets = edge.PacketsSent;
            _ = Task.Run(() => Exceptions.Ignore(() =>
                _tunnel.S2SPeerNotificationEndpointDisconnect(_tunnel.TunnelKey.SwapDirection(), EndpointId, edge.EdgeId, packets)));
        }

        private void WarnAboutLimit()
        {
            long now = DateTime.UtcNow.Ticks;
            long last = Interlocked.Read(ref _lastLimitWarningTicks);

            if (now - last > TimeSpan.FromMinutes(1).Ticks && Interlocked.CompareExchange(ref _lastLimitWarningTicks, now, last) == last)
            {
                Singletons.Logger.Warning($"Inbound UDP endpoint '{Configuration.Name}' has {Singletons.Configuration.UdpMaxSessionsPerEndpoint:n0} sessions,"
                    + " the most it allows. Datagrams from further clients are dropped until a session ends.");
            }
        }

        public EndpointConfiguration GetForDisplay() => BuildDisplayConfiguration(Direction);
    }
}
