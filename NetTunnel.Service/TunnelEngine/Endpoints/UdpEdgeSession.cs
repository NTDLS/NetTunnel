using System.Net;
using System.Net.Sockets;
using NTDLS.Helpers;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// One UDP conversation: a client (address and port) of an inbound endpoint, or the socket an outbound endpoint uses to talk to the
    /// destination for one such client. UDP has no connection, so the session lasts for as long as datagrams keep passing; the endpoint's
    /// heartbeat ends it when they stop.
    ///
    /// It behaves like a TCP edge towards the tunnel: datagrams from the tunnel arrive as numbered packets, handled concurrently and out of
    /// order, and are sent on in sequence; a disconnect from the peer says how many packets precede it. Datagram boundaries are kept, since
    /// each packet is exactly one datagram.
    /// </summary>
    internal sealed class UdpEdgeSession : IEdgeConnection
    {
        private readonly object _writeLock = new();
        private readonly Action<byte[]> _send;
        private readonly Action<UdpEdgeSession> _closed;
        private readonly IDisposable? _ownedResource;
        private readonly TimeSpan _disconnectGracePeriod;
        private readonly RmSequenceBuffer<byte[]> _sequenceBuffer = new();

        private volatile bool _isConnected = true;
        private int _closedFlag;
        private long _packetsDelivered;
        private long _closeAfterPackets = -1;
        private long _packetsSent;
        private long _lastActivityTicks = DateTime.UtcNow.Ticks;
        private ulong _bytesReceived;
        private ulong _bytesSent;
        private Timer? _closeTimer;

        /// <param name="edgeId">Identifies the session at both ends of the tunnel.</param>
        /// <param name="remoteEndPoint">The client (inbound endpoint) or the destination (outbound endpoint).</param>
        /// <param name="send">Sends one datagram to the remote end. A failure to send loses that datagram, as UDP may, and is not fatal.</param>
        /// <param name="closed">Called once, when the session ends, whichever end ended it. It must not block.</param>
        /// <param name="ownedResource">Disposed when the session ends (the destination socket of an outbound session).</param>
        public UdpEdgeSession(Guid edgeId, IPEndPoint remoteEndPoint, Action<byte[]> send, Action<UdpEdgeSession> closed,
            IDisposable? ownedResource = null, TimeSpan? disconnectGracePeriod = null)
        {
            EdgeId = edgeId;
            RemoteEndPoint = remoteEndPoint;
            _send = send;
            _closed = closed;
            _ownedResource = ownedResource;
            _disconnectGracePeriod = disconnectGracePeriod ?? TimeSpan.FromSeconds(15);
        }

        public Guid EdgeId { get; }
        public DateTime StartDateTime { get; } = DateTime.UtcNow;
        public bool IsConnected => _isConnected;
        public int ThreadId => 0;
        public IPEndPoint? RemoteEndPoint { get; }

        public DateTime LastActivityDateTime => new(Interlocked.Read(ref _lastActivityTicks), DateTimeKind.Utc);
        public double ActivityAgeInMilliseconds => (DateTime.UtcNow - LastActivityDateTime).TotalMilliseconds;
        public ulong BytesReceived => Interlocked.Read(ref _bytesReceived);
        public ulong BytesSent => Interlocked.Read(ref _bytesSent);

        /// <summary>
        /// The number of packets this side has put into the tunnel. A disconnect carries it so the peer knows how many to wait for.
        /// </summary>
        public long PacketsSent => Interlocked.Read(ref _packetsSent);

        /// <summary>
        /// The sequence number for the next datagram this side puts into the tunnel (numbers start at zero and have no gaps).
        /// </summary>
        public long NextSequence() => Interlocked.Increment(ref _packetsSent) - 1;

        /// <summary>
        /// A datagram came from the remote end of the session, on its way into the tunnel.
        /// </summary>
        public void NoteReceived(int bytes)
        {
            Interlocked.Add(ref _bytesReceived, (ulong)bytes);
            Touch();
        }

        private void Touch() => Interlocked.Exchange(ref _lastActivityTicks, DateTime.UtcNow.Ticks);

        public void Write(long packetSequence, byte[] buffer)
        {
            if (!_isConnected)
            {
                throw new ObjectDisposedException(nameof(UdpEdgeSession));
            }

            bool closeNow;

            lock (_writeLock)
            {
                _sequenceBuffer.Process(buffer, packetSequence, data =>
                {
                    try
                    {
                        _send(data);
                        Interlocked.Add(ref _bytesSent, (ulong)data.Length);
                    }
                    catch (SocketException)
                    {
                        //The datagram could not be sent (nothing is reachable, the buffer is full). UDP may lose it; the session goes on.
                    }

                    _packetsDelivered++;
                });

                closeNow = _closeAfterPackets >= 0 && _packetsDelivered >= _closeAfterPackets;
            }

            Touch();

            if (closeNow)
            {
                Disconnect();
            }
        }

        public void DisconnectAfter(long packetCount)
        {
            bool closeNow;

            lock (_writeLock)
            {
                if (packetCount >= 0)
                {
                    _closeAfterPackets = packetCount;
                }
                closeNow = packetCount < 0 || _packetsDelivered >= packetCount;
            }

            if (closeNow)
            {
                Disconnect();
                return;
            }

            //Packets that never arrive must not keep the session open for ever.
            _closeTimer = new Timer(_ => Exceptions.Ignore(Disconnect), null, _disconnectGracePeriod, Timeout.InfiniteTimeSpan);
        }

        public void Disconnect()
        {
            _isConnected = false;

            if (Interlocked.Exchange(ref _closedFlag, 1) == 1)
            {
                return; //Already ended; the owner has been told once.
            }

            Exceptions.Ignore(() => _closeTimer?.Dispose());
            Exceptions.Ignore(() => _ownedResource?.Dispose());
            Exceptions.Ignore(() => _closed(this));
        }

        public void Dispose()
        {
            Disconnect();
            Exceptions.Ignore(_sequenceBuffer.Clear);
        }
    }
}
