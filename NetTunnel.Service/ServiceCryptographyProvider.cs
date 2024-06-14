using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service
{
    internal class ServiceCryptographyProvider : IRmCryptographyProvider
    {
        private TunnelEngineCore _engineCore;

        public ServiceCryptographyProvider(TunnelEngineCore engineCore)
        {
            _engineCore = engineCore;
        }

        public byte[] Decrypt(RmContext context, byte[] encryptedPayload)
        {
            if (_engineCore.InboundTunnelConnections.TryGetValue(context.ConnectionId, out var connection))
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
            if (_engineCore.InboundTunnelConnections.TryGetValue(context.ConnectionId, out var connection))
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
