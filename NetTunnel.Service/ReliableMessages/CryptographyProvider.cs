using NTDLS.NASCCL;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableMessages
{
    public class CryptographyProvider : IRmCryptographyProvider
    {
        private readonly NASCCLStream _streamCryptography;

        public CryptographyProvider(byte[] cryptographyKey)
        {
            _streamCryptography = new NASCCLStream(cryptographyKey);
        }

        public byte[] Decrypt(RmContext context, byte[] encryptedPayload)
        {
            lock (_streamCryptography)
            {
                //Console.WriteLine($"Decrypt {encryptedPayload.Length:n0} bytes.");
                _streamCryptography.Cipher(ref encryptedPayload);
                _streamCryptography.ResetStream();
            }
            return encryptedPayload;
        }

        public byte[] Encrypt(RmContext context, byte[] payload)
        {
            lock (_streamCryptography)
            {
                //Console.WriteLine($"Encrypt {payload.Length:n0} bytes.");
                _streamCryptography.Cipher(ref payload);
                _streamCryptography.ResetStream();
            }
            return payload;
        }
    }
}
