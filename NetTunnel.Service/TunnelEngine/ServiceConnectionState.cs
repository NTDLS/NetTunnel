﻿using NetTunnel.Library;
using NTDLS.NASCCL;

namespace NetTunnel.Service.TunnelEngine
{
    public class ServiceConnectionState
    {
        public Guid ConnectionId { get; set; }
        public NASCCLStream? StreamCryptography { get; private set; }
        public bool SecureKeyExchangeIsComplete { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string UserName { get; private set; } = string.Empty;
        public string ClientIpAddress { get; private set; }
        public DateTime LoginTime { get; private set; } = DateTime.UtcNow;

        public ServiceConnectionState(Guid connectionId, string clientIpAddress)
        {
            ConnectionId = connectionId;
            ClientIpAddress = clientIpAddress;
        }

        public void InitializeCryptographyProvider(byte[] sharedSecret)
        {
            StreamCryptography = new NASCCLStream(sharedSecret);

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

        public void SetAuthenticated(string userName)
        {
            UserName = userName.ToLower();
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
