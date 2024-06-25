using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.NASCCL;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    public class ServiceConnectionState
    {
        public Guid ConnectionId { get; set; }
        public NASCCLStream? StreamCryptography { get; private set; }
        public bool SecureKeyExchangeIsComplete { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string UserName { get; private set; } = string.Empty;
        public NtUserRole UserRole { get; set; } = NtUserRole.Undefined;
        public string ClientIpAddress { get; private set; }
        public DateTime LoginTime { get; private set; } = DateTime.UtcNow;
        public string KeyHash { get; private set; } = string.Empty;
        public int KeyLength { get; private set; }

        /// <summary>
        /// If the Service Connection is associated with a tunnel connection, this will be set at tunnel registration.
        /// Remember that the UI also makes connections to the ServiceEngine, and those connections do not use a tunnel.
        /// </summary>
        public DirectionalKey? TunnelKey { get; set; }

        public ServiceConnectionState(Guid connectionId, string clientIpAddress)
        {
            ConnectionId = connectionId;
            ClientIpAddress = clientIpAddress;
        }

        public void AssociateTunnel(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }

        public void InitializeCryptographyProvider(byte[] sharedSecret)
        {
            StreamCryptography = new NASCCLStream(sharedSecret);

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

            SecureKeyExchangeIsComplete = true;
        }

        public void SetAuthenticated(string userName, NtUserRole role)
        {
            UserName = userName.ToLower();
            UserRole = role;
            IsAuthenticated = true;
        }

        public bool Validate(string? clientIpAddress)
        {
            if (ClientIpAddress != clientIpAddress)
            {
                throw new Exception("Session IP address mismatch.");
            }

            return true;
        }
    }
}
