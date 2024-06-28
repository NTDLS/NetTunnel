using System.Security.Cryptography;
using System.Text;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library
{
    public static class Utility
    {
        public delegate void ServiceLogDelegate(DateTime dateTime, NtLogSeverity severity, string message);

        public static int CombineHashes(int hash1, int hash2)
        {
            uint rol5 = ((uint)hash1 << 5) | ((uint)hash1 >> 27);
            return ((int)rol5 + hash1) ^ hash2;
        }

        public static int CombineHashes(int[] hashes)
        {
            int hashCount = hashes.Length;
            if (hashCount > 1)
            {
                int runningHash = hashes[0];

                for (int i = 1; i < hashCount; i++)
                {
                    runningHash = CombineHashes(runningHash, hashes[i]);
                }
                return runningHash;
            }
            else if (hashCount > 0)
            {
                return hashes[0];
            }

            return 0;
        }

        public static string ComputeSha256Hash(string input)
            => ComputeSha256Hash(Encoding.UTF8.GetBytes(input));

        public static string ComputeSha256Hash(byte[] inputBytes)
        {
            var stringBuilder = new StringBuilder();

            var hashBytes = SHA256.HashData(inputBytes);
            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}

