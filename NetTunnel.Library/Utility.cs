
using NetTunnel.Library.Exceptions;
using ProtoBuf;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace NetTunnel.Library
{
    public static class Utility
    {
        public static string CalculateSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            return (new StackTrace())?.GetFrame(1)?.GetMethod()?.Name ?? "{unknown frame}";
        }

        public delegate void TryAndIgnoreProc();
        public delegate T TryAndIgnoreProc<T>();

        /// <summary>
        /// We didnt need that exception! Did we?... DID WE?!
        /// </summary>
        public static void TryAndIgnore(TryAndIgnoreProc func)
        {
            try { func(); } catch { }
        }

        /// <summary>
        /// We didnt need that exception! Did we?... DID WE?!
        /// </summary>
        public static T? TryAndIgnore<T>(TryAndIgnoreProc<T> func)
        {
            try { return func(); } catch { }
            return default;
        }

        public static void EnsureNotNull<T>([NotNull] T? value, string? message = null, [CallerArgumentExpression(nameof(value))] string strName = "")
        {
            if (value == null)
            {
                if (message == null)
                {
                    throw new NtNullException($"Value should not be null: '{strName}'.");
                }
                else
                {
                    throw new NtNullException(message);
                }
            }
        }

        public static void EnsureNotNullOrEmpty([NotNull] Guid? value, [CallerArgumentExpression(nameof(value))] string strName = "")
        {
            if (value == null || value == Guid.Empty)
            {
                throw new NtNullException($"Value should not be null or empty: '{strName}'.");
            }
        }

        public static void EnsureNotNullOrEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string strName = "")
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new NtNullException($"Value should not be null or empty: '{strName}'.");
            }
        }

        public static void EnsureNotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string strName = "")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new NtNullException($"Value should not be null or empty: '{strName}'.");
            }
        }

        [Conditional("DEBUG")]
        public static void AssertIfDebug(bool condition, string message)
        {
            if (condition)
            {
                throw new NtAssertException(message);
            }
        }

        public static void Assert(bool condition, string message)
        {
            if (condition)
            {
                throw new NtAssertException(message);
            }
        }

        public static byte[] SerializeToByteArray(object obj)
        {
            if (obj == null) return Array.Empty<byte>();
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);
            return stream.ToArray();
        }

        public static T DeserializeToObject<T>(byte[] arrBytes)
        {
            using var stream = new MemoryStream();
            stream.Write(arrBytes, 0, arrBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize<T>(stream);
        }

        public static byte[] Compress(byte[]? bytes)
        {
            if (bytes == null) return Array.Empty<byte>();

            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }
            return mso.ToArray();
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }
            return mso.ToArray();
        }
    }
}
