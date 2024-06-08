using NTDLS.NASCCL;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.FramePayloads
{
    public class EncryptionProvider : IRmEncryptionProvider
    {
        private readonly NASCCLStream _streamCryptography;

        public EncryptionProvider(byte[] encryptionKey)
        {
            _streamCryptography = new NASCCLStream(encryptionKey);
        }

        public byte[] Decrypt(byte[] encryptedPayload)
        {
            lock (_streamCryptography)
            {
                Console.WriteLine($"Decrypt {encryptedPayload.Length:n0} bytes.");
                _streamCryptography.Cipher(ref encryptedPayload);
                _streamCryptography.ResetStream();
            }
            return encryptedPayload;
        }

        public byte[] Encrypt(byte[] payload)
        {
            lock (_streamCryptography)
            {
                Console.WriteLine($"Encrypt {payload.Length:n0} bytes.");
                _streamCryptography.Cipher(ref payload);
                _streamCryptography.ResetStream();
            }
            return payload;
        }
    }
}
