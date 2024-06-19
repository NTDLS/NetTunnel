using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace NetTunnel.Library
{
    public static class Utility
    {
        public static int CombineHashes(int hash1, int hash2)
        {
            uint rol5 = ((uint)hash1 << 5) | ((uint)hash1 >> 27);
            return ((int)rol5 + hash1) ^ hash2;
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

        public delegate void TryAndIgnoreProc();
        /// <summary>
        /// We didn't need that exception! Did we?... DID WE?!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryAndIgnore(TryAndIgnoreProc func)
        {
            try { func(); } catch { }
        }

        public delegate T TryAndIgnoreProc<T>();
        /// <summary>
        /// We didn't need that exception! Did we?... DID WE?!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? TryAndIgnore<T>(TryAndIgnoreProc<T> func)
        {
            try { return func(); } catch { }
            return default;
        }
    }
}
