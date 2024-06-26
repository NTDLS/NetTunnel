﻿using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine
{
    internal class ServiceCryptographyProvider : IRmCryptographyProvider
    {
        private readonly ServiceEngine _serviceEngine;

        public ServiceCryptographyProvider(ServiceEngine serviceEngine)
        {
            _serviceEngine = serviceEngine;
        }

        public byte[] Decrypt(RmContext context, byte[] encryptedPayload)
        {
            if (_serviceEngine.TryGetServiceConnectionState(context.ConnectionId, out var connection))
            {
                if (connection.TunnelKey != null)
                {
                    _serviceEngine.Tunnels.IncrementBytesReceived(connection.TunnelKey, encryptedPayload.Length);
                }

                if (connection.StreamCryptography != null && connection.IsKeyExchangeComplete)
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
            if (_serviceEngine.TryGetServiceConnectionState(context.ConnectionId, out var connection))
            {
                if (connection.TunnelKey != null)
                {
                    _serviceEngine.Tunnels.IncrementBytesSent(connection.TunnelKey, payload.Length);
                }

                if (connection.StreamCryptography != null && connection.IsKeyExchangeComplete)
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
