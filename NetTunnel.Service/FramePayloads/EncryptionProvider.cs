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
            //return encryptedPayload;
            lock (_streamCryptography)
            {
                _streamCryptography.Cipher(ref encryptedPayload);
                _streamCryptography.ResetStream();
            }
            return encryptedPayload;
        }

        public byte[] Encrypt(byte[] payload)
        {
            //return payload;
            lock (_streamCryptography)
            {
                _streamCryptography.Cipher(ref payload);
                _streamCryptography.ResetStream();
            }
            return payload;
        }
    }
}
