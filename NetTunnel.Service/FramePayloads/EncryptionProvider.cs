﻿using NTDLS.NASCCL;
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
                Console.WriteLine($"Decrypt Key CRC: {CRC16.ComputeChecksum(_streamCryptography._keyBuffer)}");

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
                Console.WriteLine($"Encrypt Key CRC: {CRC16.ComputeChecksum(_streamCryptography._keyBuffer)}");

                _streamCryptography.Cipher(ref payload);
                _streamCryptography.ResetStream();
            }
            return payload;
        }


        internal static class CRC16
        {
            private const ushort polynomial = 0xAB01;
            private static readonly ushort[] table = new ushort[256];

            public static ushort ComputeChecksum(byte[] bytes)
            {
                ushort crc = 0;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    byte index = (byte)(crc ^ bytes[i]);
                    crc = (ushort)(crc >> 8 ^ table[index]);
                }
                return crc;
            }

            public static ushort ComputeChecksum(byte[] bytes, int offset, int length)
            {
                ushort crc = 0;
                for (int i = offset; i < length + offset; ++i)
                {
                    byte index = (byte)(crc ^ bytes[i]);
                    crc = (ushort)(crc >> 8 ^ table[index]);
                }
                return crc;
            }

            static CRC16()
            {
                ushort value;
                ushort temp;
                for (ushort i = 0; i < table.Length; ++i)
                {
                    value = 0;
                    temp = i;
                    for (byte j = 0; j < 8; ++j)
                    {
                        if (((value ^ temp) & 0x0001) != 0)
                        {
                            value = (ushort)(value >> 1 ^ polynomial);
                        }
                        else
                        {
                            value >>= 1;
                        }
                        temp >>= 1;
                    }
                    table[i] = value;
                }
            }
        }
    }
}
