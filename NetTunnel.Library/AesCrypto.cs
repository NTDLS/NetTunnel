using System.Security.Cryptography;
using System.Text;

namespace NetTunnel.Library
{
    public class AesCrypto(string encryptionKey)
    {
        // 32 bytes = 256-bit key.
        private readonly byte[] _key = SHA256.HashData(Encoding.UTF8.GetBytes(encryptionKey));

        public byte[] Encrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var cipherText = encryptor.TransformFinalBlock(data, 0, data.Length);

            // Prepend IV to ciphertext
            return aes.IV.Concat(cipherText).ToArray();
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            using var aes = Aes.Create();
            aes.Key = _key;

            // Extract IV (first 16 bytes)
            var iv = encryptedData.Take(16).ToArray();
            var cipherText = encryptedData.Skip(16).ToArray();

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        }

        public string EncryptString(string plainText)
        {
            var buffer = Encoding.UTF8.GetBytes(plainText);
            var encryptedData = Encrypt(buffer);
            return Convert.ToBase64String(encryptedData);
        }

        public string DecryptString(string base64CipherText)
        {
            var buffer = Convert.FromBase64String(base64CipherText);
            var bytes = Decrypt(buffer);
            return Encoding.UTF8.GetString(bytes);
        }

        public string Sha1(string text)
            => Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(text)));

        public string Sha256(string text)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

        /// <summary>
        /// Generates a registration code for the given role name.
        /// </summary>
        public string GenRegCode()
        {
            var chars = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
            return $"{chars[..4]}-{chars[4..8]}-{chars[8..12]}";
        }

        public string GenPasswordResetCode()
        {
            var chars = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
            return $"{chars[..4]}-{chars[4..8]}";
        }
    }
}
