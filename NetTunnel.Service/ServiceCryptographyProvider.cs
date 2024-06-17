﻿using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service
{
    internal class ServiceCryptographyProvider : IRmCryptographyProvider
    {
        private readonly ServiceEngine _serviceEngine;

        public ServiceCryptographyProvider(ServiceEngine engineCore)
        {
            _serviceEngine = engineCore;
        }

        public byte[] Decrypt(RmContext context, byte[] encryptedPayload)
        {
            if (_serviceEngine.ServiceConnectionStates.TryGetValue(context.ConnectionId, out var connection))
            {
                if (connection.StreamCryptography != null && connection.SecureKeyExchangeIsComplete)
                {
                    lock (connection.StreamCryptography)
                    {
                        connection.StreamCryptography.Cipher(ref encryptedPayload);
                        connection.StreamCryptography.ResetStream();
                    }
                }
            }
            return encryptedPayload;
        }

        public byte[] Encrypt(RmContext context, byte[] payload)
        {
            if (_serviceEngine.ServiceConnectionStates.TryGetValue(context.ConnectionId, out var connection))
            {
                if (connection.StreamCryptography != null && connection.SecureKeyExchangeIsComplete)
                {
                    lock (connection.StreamCryptography)
                    {
                        connection.StreamCryptography.Cipher(ref payload);
                        connection.StreamCryptography.ResetStream();
                    }
                }
            }
            return payload;
        }
    }
}
