using System.Net;
using System.Net.Sockets;
using NetTunnel.Library;
using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// The UDP counterpart of <see cref="EndpointOutbound"/>: when a client of the inbound endpoint is seen for the first time it opens
    /// a socket of its own to the destination (so that the destination can tell the clients apart and answers the right one), sends
    /// the datagrams of the client through it, and puts what the destination answers into the tunnel.
    /// </summary>
    internal class UdpEndpointOutbound : BaseEndpoint, IEndpoint, IOutboundEndpoint
    {
        public DirectionalKey EndpointKey => new(this);
        public NtDirection Direction => NtDirection.Outbound;
        public override int GetHashCode() => Configuration.GetHashCode();

        public UdpEndpointOutbound(IServiceEngine serviceEngine, ITunnel tunnel, EndpointConfiguration configuration)
            : base(serviceEngine, tunnel, configuration.EndpointId, configuration)
        {
        }

        public override void Start()
        {
            if (TryBeginStart() == false)
            {
                return; //Already running.
            }

            Singletons.Logger.Verbose($"Starting outbound UDP endpoint '{Configuration.Name}' to port {Configuration.OutboundPort}.");
        }

        public override void Stop()
        {
            if (TryBeginStop() == false)
            {
                return; //Not running.
            }

            Singletons.Logger.Verbose($"Stopping outbound UDP endpoint '{Configuration.Name}' to port {Configuration.OutboundPort}.");

            foreach (var edge in _edgeConnections.Use(o => o.Values.ToList()))
            {
                Exceptions.Ignore(edge.Disconnect);
            }

            Singletons.Logger.Verbose($"Stopped outbound UDP endpoint '{Configuration.Name}' to port {Configuration.OutboundPort}.");
        }

        /// <summary>
        /// The other end of the tunnel has seen a new client and asks for the session to the destination.
        /// </summary>
        public void EstablishOutboundEndpointConnection(Guid edgeId, string? applicationProtocol = null)
        {
            if (KeepRunning == false)
            {
                return;
            }

            if (_edgeConnections.Use(o => o.Count) >= Singletons.Configuration.UdpMaxSessionsPerEndpoint)
            {
                throw new InvalidOperationException($"Outbound UDP endpoint '{Configuration.Name}' has too many sessions.");
            }

            var socket = NetworkUtility.ConnectUdp(Configuration.OutboundAddress, Configuration.OutboundPort);

            UdpEdgeSession edge;
            try
            {
                edge = new UdpEdgeSession(edgeId, (IPEndPoint)socket.RemoteEndPoint!, data => socket.Send(data), OnSessionClosed, ownedResource: socket);

                if (_edgeConnections.Use(o => o.TryAdd(edgeId, edge)) == false)
                {
                    throw new InvalidOperationException($"UDP session {edgeId} already exists.");
                }
            }
            catch
            {
                socket.Dispose();
                throw;
            }

            NoteConnectionOpened();
            Singletons.Logger.Debug($"Established outbound UDP session {edgeId} to {edge.RemoteEndPoint}");

            if (KeepRunning == false) //Check again, we may have been stopped meanwhile.
            {
                edge.Disconnect();
                return;
            }

            _ = Task.Run(() => ReceiveFromDestinationAsync(edge, socket));
        }

        /// <summary>
        /// Puts what the destination sends to this session into the tunnel, as numbered packets, one per datagram.
        /// </summary>
        private async Task ReceiveFromDestinationAsync(UdpEdgeSession edge, Socket socket)
        {
            var buffer = new byte[UdpMaxDatagramSize];

            while (edge.IsConnected)
            {
                int received;

                try
                {
                    received = await socket.ReceiveAsync(buffer, SocketFlags.None);
                }
                catch (ObjectDisposedException)
                {
                    break; //The session ended and closed the socket.
                }
                catch (SocketException ex) when (!edge.IsConnected || ex.SocketErrorCode == SocketError.OperationAborted)
                {
                    break;
                }
                catch (SocketException ex) when (ex.SocketErrorCode is SocketError.ConnectionReset or SocketError.ConnectionRefused
                    or SocketError.MessageSize or SocketError.NetworkReset or SocketError.HostUnreachable or SocketError.NetworkUnreachable)
                {
                    //The network said an earlier datagram did not arrive (an ICMP error). The destination may start answering later,
                    //  and the datagrams that follow are not affected, so the session goes on.
                    await Task.Delay(10);
                    continue;
                }
                catch (Exception ex)
                {
                    Singletons.Logger.Warning($"Outbound UDP endpoint '{Configuration.Name}' failed to receive from {edge.RemoteEndPoint}: {ex.Message}");
                    edge.Disconnect();
                    break;
                }

                if (received == 0)
                {
                    continue;
                }

                edge.NoteReceived(received);
                AddBytesReceived(received);

                try
                {
                    _tunnel.S2SPeerNotificationEndpointDataExchange(
                        _tunnel.TunnelKey, EndpointId, edge.EdgeId, edge.NextSequence(), buffer, received);
                }
                catch (Exception ex)
                {
                    Singletons.Logger.Verbose($"Outbound UDP session {edge.EdgeId} could not reach the tunnel: {ex.Message}");
                    edge.Disconnect();
                    break;
                }
            }
        }

        private void OnSessionClosed(UdpEdgeSession edge)
        {
            _edgeConnections.Use(o => o.Remove(edge.EdgeId));
            NoteConnectionClosed();

            Singletons.Logger.Debug($"Ended outbound UDP session {edge.EdgeId} to {edge.RemoteEndPoint}");

            //Not on this thread: it may be one that holds a lock, and the notification goes over the network.
            long packets = edge.PacketsSent;
            _ = Task.Run(() => Exceptions.Ignore(() =>
                _tunnel.S2SPeerNotificationEndpointDisconnect(_tunnel.TunnelKey.SwapDirection(), EndpointId, edge.EdgeId, packets)));
        }

        public EndpointConfiguration GetForDisplay() => BuildDisplayConfiguration(Direction);
    }
}
