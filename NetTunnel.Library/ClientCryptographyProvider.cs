using NetTunnel.Library.Interfaces;
using NTDLS.NASCCL;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableMessages
{
    public class ClientCryptographyProvider : IRmCryptographyProvider
    {
        private readonly NASCCLStream _streamCryptography;

        public ClientCryptographyProvider(byte[] cryptographyKey)
        {
            _streamCryptography = new NASCCLStream(cryptographyKey);
        }

        public byte[] Decrypt(RmContext context, byte[] encryptedPayload)
        {
            lock (_streamCryptography)
            {
                if (context.Endpoint.Parameter is ITunnel tunnel)
                {
                    tunnel.IncrementBytesReceived(encryptedPayload.Length);
                }

                _streamCryptography.Cipher(ref encryptedPayload);
                _streamCryptography.ResetStream();
            }
            return encryptedPayload;
        }

        public byte[] Encrypt(RmContext context, byte[] payload)
        {
            lock (_streamCryptography)
            {
                if (context.Endpoint.Parameter is ITunnel tunnel)
                {
                    tunnel.IncrementBytesSent(payload.Length);
                }

                _streamCryptography.Cipher(ref payload);
                _streamCryptography.ResetStream();
            }
            return payload;
        }
    }
}
