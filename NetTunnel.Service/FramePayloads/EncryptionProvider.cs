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
            _streamCryptography.Cipher(ref encryptedPayload);
            return encryptedPayload;
        }

        public byte[] Encrypt(byte[] payload)
        {
            _streamCryptography.Cipher(ref payload);
            return payload;
        }
    }
}
