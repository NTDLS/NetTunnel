using System.Security.Cryptography;
using System.Text;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public static class Utility
    {
        public delegate void ServiceLogDelegate(DateTime dateTime, NtLogSeverity severity, string message);

        /// <summary>
        /// Trims a host name or IP address as typed by a person, and removes the square brackets that IPv6 addresses are often written
        /// with ("[::1]"), which name resolution does not accept.
        /// </summary>
        public static string NormalizeHost(string host)
        {
            host = host.Trim();

            if (host.Length > 1 && host[0] == '[' && host[^1] == ']')
            {
                host = host[1..^1];
            }

            return host;
        }

        /// <summary>
        /// Formats a host name or IP address and port for display ("example.com:80", "[::1]:80").
        /// </summary>
        public static string FormatHostPort(string host, int port)
        {
            host = NormalizeHost(host);
            return host.Contains(':') ? $"[{host}]:{port}" : $"{host}:{port}";
        }

        /// <summary>
        /// Formats the address of a connection for display. An IPv4 address that arrived on a dual-stack socket
        /// ("::ffff:192.168.1.2") is shown as the IPv4 address it is.
        /// </summary>
        public static string FormatEndPoint(System.Net.IPEndPoint endPoint)
            => FormatHostPort(Unmap(endPoint.Address).ToString(), endPoint.Port);

        /// <summary>
        /// Returns the IPv4 address for an IPv4-mapped IPv6 address, and any other address as it is.
        /// </summary>
        public static System.Net.IPAddress Unmap(System.Net.IPAddress address)
            => address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

        public static int CombineHashes(int hash1, int hash2)
        {
            uint rol5 = ((uint)hash1 << 5) | ((uint)hash1 >> 27);
            return ((int)rol5 + hash1) ^ hash2;
        }

        public static int CombineHashes(int[] hashes)
        {
            int hashCount = hashes.Length;
            if (hashCount > 1)
            {
                int runningHash = hashes[0];

                for (int i = 1; i < hashCount; i++)
                {
                    runningHash = CombineHashes(runningHash, hashes[i]);
                }
                return runningHash;
            }
            else if (hashCount > 0)
            {
                return hashes[0];
            }

            return 0;
        }

        public static string ComputeSha256Hash(string input)
            => ComputeSha256Hash(Encoding.UTF8.GetBytes(input));

        public static string ComputeSha256Hash(byte[] inputBytes)
        {
            var stringBuilder = new StringBuilder();

            var hashBytes = SHA256.HashData(inputBytes);
            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}

