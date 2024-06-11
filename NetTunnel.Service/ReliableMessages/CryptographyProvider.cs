using NetTunnel.Library;
using NetTunnel.Service.TunnelEngine.Tunnels;
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
            //var tunnel = (context.Endpoint.Parameter as ITunnel).EnsureNotNull();
            //tunnel.BytesReceived += (ulong)encryptedPayload.Length;

            lock (_streamCryptography)
            {
                _streamCryptography.Cipher(ref encryptedPayload);
                _streamCryptography.ResetStream();
            }
            return encryptedPayload;
        }

        public byte[] Encrypt(RmContext context, byte[] payload)
        {
            //var tunnel = (context.Endpoint.Parameter as ITunnel).EnsureNotNull();
            //tunnel.BytesSent += (ulong)payload.Length;

            lock (_streamCryptography)
            {
                _streamCryptography.Cipher(ref payload);
                _streamCryptography.ResetStream();
            }
            return payload;
        }
    }
}
