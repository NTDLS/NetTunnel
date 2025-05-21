using NetTunnel.Library.Interfaces;
using NTDLS.Permafrost;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library
{
    public class ClientCryptographyProvider : IRmCryptographyProvider
    {
        private readonly PermafrostCipher _streamCryptography;

        public ClientCryptographyProvider(byte[] cryptographyKey)
        {
            _streamCryptography = new PermafrostCipher(cryptographyKey, PermafrostMode.AutoReset);
        }

        public byte[] Decrypt(RmContext context, byte[] encryptedPayload)
        {
            lock (_streamCryptography)
            {
                if (context.Messenger.Parameter is ITunnel tunnel)
                {
                    tunnel.IncrementBytesReceived(encryptedPayload.Length);
                }

                _streamCryptography.Cipher(encryptedPayload);
            }
            return encryptedPayload;
        }

        public byte[] Encrypt(RmContext context, byte[] payload)
        {
            lock (_streamCryptography)
            {
                if (context.Messenger.Parameter is ITunnel tunnel)
                {
                    tunnel.IncrementBytesSent(payload.Length);
                }

                //_streamCryptography.Cipher(payload);
            }
            return payload;
        }
    }
}
