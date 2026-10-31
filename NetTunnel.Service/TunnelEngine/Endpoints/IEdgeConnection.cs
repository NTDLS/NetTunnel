using System.Net;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// What an endpoint needs of one conversation with a client or a destination, whether it is a TCP connection or a UDP session
    /// (a client address and port that keeps sending). Data from the tunnel arrives as numbered packets, from concurrent handlers and
    /// out of order, and is written in sequence. A disconnect from the peer says how many packets precede it, so that none are lost.
    /// </summary>
    internal interface IEdgeConnection : IDisposable
    {
        Guid EdgeId { get; }
        bool IsConnected { get; }
        DateTime LastActivityDateTime { get; }
        DateTime StartDateTime { get; }
        double ActivityAgeInMilliseconds { get; }
        ulong BytesReceived { get; }
        ulong BytesSent { get; }

        /// <summary>
        /// The thread which serves the connection, or zero when it has none of its own (UDP sessions).
        /// </summary>
        int ThreadId { get; }

        /// <summary>
        /// The address and port of the other end, or null if it cannot be known (for example the socket has already closed).
        /// </summary>
        IPEndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// Writes a packet received from the tunnel, in sequence with the ones before it.
        /// </summary>
        void Write(long packetSequence, byte[] buffer);

        /// <summary>
        /// Closes the connection once <paramref name="packetCount"/> packets have been written. Negative closes it at once.
        /// </summary>
        void DisconnectAfter(long packetCount);

        void Disconnect();
    }

    /// <summary>
    /// An endpoint which makes the connection (or opens the session) to the destination when the other end of the tunnel asks it to.
    /// </summary>
    internal interface IOutboundEndpoint
    {
        void EstablishOutboundEndpointConnection(Guid edgeId, string? applicationProtocol = null);
    }
}
