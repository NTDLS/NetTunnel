using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.NASCCL;
using System.Net;
using System.Net.Sockets;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// This class serves a dual purpose, first and foremost it is used as the connection state for the inbound
    /// tunnel - most importantly housing the StreamCryptography.
    /// 
    /// It is also used for general status information such as whether the key exchange is complete, the login type, role, etc.
    /// This second purpose is used by both the inbound and bound services, but the outbound service only uses this information
    /// for display purposes whereas the inbound services uses it for internal state (and display).
    /// </summary>
    public class ServiceConnectionState
    {
        public bool IsAuthenticated { get; private set; }
        public bool IsKeyExchangeComplete { get; private set; }
        public CryptoStream? StreamCryptography { get; private set; }
        public DateTime LoginTime { get; private set; } = DateTime.UtcNow;
        /// <summary>
        /// If the Service Connection is associated with a tunnel connection, this will be set at tunnel registration.
        /// Remember that the UI also makes connections to the ServiceEngine, and those connections do not use a tunnel.
        /// </summary>
        public DirectionalKey? TunnelKey { get; set; }
        public Guid ConnectionId { get; set; }
        public int KeyLength { get; private set; }
        public NtLoginType LoginType { get; set; } = NtLoginType.Undefined;
        public NtUserRole UserRole { get; set; } = NtUserRole.Undefined;

        /// <summary>
        /// Used to test for IP spoofing.
        /// </summary>
        public string TunnelAddressIdentifier { get; private set; }
        public string LocalClientAddress { get; private set; }
        public string RemoteClientAddress { get; private set; }
        public string KeyHash { get; private set; } = string.Empty;
        public string UserName { get; private set; } = string.Empty;

        public ServiceConnectionState(Guid connectionId, Socket nativeSocket)
        {
            ConnectionId = connectionId;

            if (nativeSocket.LocalEndPoint is IPEndPoint localIpEndPoint)
            {
                LocalClientAddress = $"{localIpEndPoint.Address}:{localIpEndPoint.Port}";
            }
            else
            {
                LocalClientAddress = $"{nativeSocket.LocalEndPoint}";
            }

            if (nativeSocket.RemoteEndPoint is IPEndPoint remoteIpEndPoint)
            {
                RemoteClientAddress = $"{remoteIpEndPoint.Address}:{remoteIpEndPoint.Port}";
            }
            else
            {
                RemoteClientAddress = $"{nativeSocket.LocalEndPoint}";
            }

            TunnelAddressIdentifier = $"[{LocalClientAddress}]/[{RemoteClientAddress}]";
        }

        public void AssociateTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }

        public void InitializeCryptographyProvider(byte[] sharedSecret)
        {
            StreamCryptography = new CryptoStream(sharedSecret);

            KeyHash = Utility.ComputeSha256Hash(sharedSecret);
            KeyLength = sharedSecret.Length;

            Singletons.Logger.Verbose(
                    $"Tunnel cryptography initialized to {sharedSecret.Length * 8}bits. Hash {Utility.ComputeSha256Hash(sharedSecret)}.");
        }

        public void InformCryptographyProvider(byte[] sharedSecret)
        {
            KeyHash = Utility.ComputeSha256Hash(sharedSecret);
            KeyLength = sharedSecret.Length;

            Singletons.Logger.Verbose(
                    $"Tunnel cryptography initialized to {sharedSecret.Length * 8}bits. Hash {Utility.ComputeSha256Hash(sharedSecret)}.");
        }

        public void ApplyCryptographyProvider()
        {
            if (StreamCryptography == null)
            {
                throw new Exception("The stream cryptography has not been initialized.");
            }

            Singletons.Logger.Verbose("Tunnel cryptography provider has been applied.");

            IsKeyExchangeComplete = true;
        }

        public void SetAuthenticated(string userName, NtUserRole role, NtLoginType loginType)
        {
            UserName = userName.ToLower();
            UserRole = role;
            LoginType = loginType;
            IsAuthenticated = true;
        }

        public bool Validate(Socket nativeSocket)
        {
            if (nativeSocket.LocalEndPoint is IPEndPoint localIpEndPoint)
            {
                LocalClientAddress = $"{localIpEndPoint.Address}:{localIpEndPoint.Port}";
            }
            else
            {
                LocalClientAddress = $"{nativeSocket.LocalEndPoint}";
            }

            if (nativeSocket.RemoteEndPoint is IPEndPoint remoteIpEndPoint)
            {
                RemoteClientAddress = $"{remoteIpEndPoint.Address}:{remoteIpEndPoint.Port}";
            }
            else
            {
                RemoteClientAddress = $"{nativeSocket.LocalEndPoint}";
            }

            if (!$"[{LocalClientAddress}]/[{RemoteClientAddress}]".Equals(TunnelAddressIdentifier, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Session IP address mismatch.");
            }

            return true;
        }
    }
}
