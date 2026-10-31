using NetTunnel.Library;
using System.Net;
using System.Net.Sockets;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// Creates the sockets that endpoints use, so that IPv4 and IPv6 are handled in one place.
    /// </summary>
    internal static class NetworkUtility
    {
        /// <summary>
        /// Whether endpoints should use IPv6 at all: it is turned on in the service configuration and the operating system has it.
        /// </summary>
        public static bool IPv6Enabled => Singletons.Configuration.EnableIPv6 && Socket.OSSupportsIPv6;

        /// <summary>
        /// A listener for an inbound endpoint. With IPv6 it is one dual-stack listener that accepts IPv6 and IPv4 clients alike.
        /// </summary>
        public static TcpListener CreateListener(int port)
        {
            if (IPv6Enabled)
            {
                var listener = new TcpListener(IPAddress.IPv6Any, port);
                listener.Server.DualMode = true;
                return listener;
            }

            return new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// A bound UDP socket for an inbound endpoint. With IPv6 it is one dual-stack socket that receives from IPv6 and IPv4 clients alike.
        /// </summary>
        public static Socket CreateUdpListenSocket(int port)
        {
            Socket socket;

            if (IPv6Enabled)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp) { DualMode = true };
                    socket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
                    PrepareUdpSocket(socket);
                    return socket;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressFamilyNotSupported)
                {
                    //IPv6 is installed but not usable here. Fall through to IPv4 only.
                    Singletons.Logger.Warning("IPv6 is not available for a UDP endpoint, listening on IPv4 only.");
                }
            }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch
            {
                socket.Dispose();
                throw;
            }

            PrepareUdpSocket(socket);
            return socket;
        }

        /// <summary>
        /// A UDP socket which sends to, and only receives from, a destination (a host name, IPv4 address or IPv6 address).
        /// </summary>
        public static Socket ConnectUdp(string host, int port)
        {
            host = Utility.NormalizeHost(host);

            var addresses = IPAddress.TryParse(host, out var literal) ? [literal] : Dns.GetHostAddresses(host);
            var usable = IPv6Enabled ? addresses : addresses.Where(o => o.AddressFamily == AddressFamily.InterNetwork).ToArray();

            if (usable.Length == 0)
            {
                throw new InvalidOperationException($"'{host}' has no IPv4 address, and IPv6 is turned off in the service configuration.");
            }

            //A name can have both kinds of address; prefer IPv4 as the datagrams of a UDP session cannot be retried on the next one.
            var address = usable.FirstOrDefault(o => o.AddressFamily == AddressFamily.InterNetwork) ?? usable[0];

            var socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Connect(new IPEndPoint(address, port));
                PrepareUdpSocket(socket);
                return socket;
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        private static void PrepareUdpSocket(Socket socket)
        {
            try { socket.ReceiveBufferSize = 4 * 1024 * 1024; } catch { /* the operating system may refuse; the default is used */ }

            if (OperatingSystem.IsWindows())
            {
                //Windows reports an ICMP "port unreachable" as an error on the next receive, which would end the receive loop.
                //  A datagram that nobody wanted says nothing about the datagrams which follow.
                const int SIO_UDP_CONNRESET = -1744830452;
                try { socket.IOControl(SIO_UDP_CONNRESET, [0], null); } catch { /* not supported on this socket */ }
            }
        }

        /// <summary>
        /// Connects to a host name, IPv4 address or IPv6 address. The addresses a host name resolves to are tried in turn. When IPv6 is
        /// turned off only the IPv4 ones are used.
        /// </summary>
        public static TcpClient Connect(string host, int port)
        {
            host = Utility.NormalizeHost(host);

            //Created without an address family so that it is a dual-stack socket when the operating system has IPv6.
            var client = new TcpClient();

            try
            {
                if (IPv6Enabled)
                {
                    client.Connect(host, port);
                }
                else
                {
                    var addresses = IPAddress.TryParse(host, out var literal) ? [literal] : Dns.GetHostAddresses(host);
                    var ipv4 = addresses.Where(o => o.AddressFamily == AddressFamily.InterNetwork).ToArray();

                    if (ipv4.Length == 0)
                    {
                        throw new InvalidOperationException($"'{host}' has no IPv4 address, and IPv6 is turned off in the service configuration.");
                    }

                    client.Connect(ipv4, port);
                }

                return client;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }
    }
}
