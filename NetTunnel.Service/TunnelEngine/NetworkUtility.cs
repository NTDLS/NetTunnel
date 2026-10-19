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
