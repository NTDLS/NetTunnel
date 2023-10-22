using NetTunnel.Library.Types;
using ProtoBuf;
using System.IO.Compression;
using static NetTunnel.Service.Packets.Constants;

namespace NetTunnel.Service.Packets
{
    /// <summary>
    /// TCP packets can be fragmented or combined. The packetizer rebuilds what was originally
    /// sent via the TCP send() call, provides compression and also performs a CRC check.
    /// </summary>
    internal static class Packetizer
    {
        public delegate void ProcessPacketCallback(ITunnel tunnel, NtPacket packet);

        public static byte[] AssemblePacket(NtPacket packet)
        {
            try
            {
                var packetBody = ToByteArray(packet);
                var packetBytes = Compress(packetBody);
                var grossPacketSize = packetBytes.Length + Sanity.PACKET_HEADER_SIZE;
                var grossPacketBytes = new byte[grossPacketSize];
                var packetCrc = CRC16.ComputeChecksum(packetBytes);

                Buffer.BlockCopy(BitConverter.GetBytes(Sanity.PACKET_DELIMITER), 0, grossPacketBytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(grossPacketSize), 0, grossPacketBytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(packetCrc), 0, grossPacketBytes, 8, 2);
                Buffer.BlockCopy(packetBytes, 0, grossPacketBytes, Sanity.PACKET_HEADER_SIZE, packetBytes.Length);

                return grossPacketBytes;
            }
            catch (Exception ex)
            {
                //LogException(ex);
                throw;
            }
        }

        private static void SkipPacket(ref NtPacketBuffer packetBuffer)
        {
            try
            {
                var packetDelimiterBytes = new byte[4];

                for (int offset = 1; offset < packetBuffer.PacketBuilderLength - packetDelimiterBytes.Length; offset++)
                {
                    Buffer.BlockCopy(packetBuffer.PacketBuilder, offset, packetDelimiterBytes, 0, packetDelimiterBytes.Length);

                    var value = BitConverter.ToInt32(packetDelimiterBytes, 0);

                    if (value == Sanity.PACKET_DELIMITER)
                    {
                        Buffer.BlockCopy(packetBuffer.PacketBuilder, offset, packetBuffer.PacketBuilder, 0, packetBuffer.PacketBuilderLength - offset);
                        packetBuffer.PacketBuilderLength -= offset;
                        return;
                    }
                }
                Array.Clear(packetBuffer.PacketBuilder, 0, packetBuffer.PacketBuilder.Length);
                packetBuffer.PacketBuilderLength = 0;
            }
            catch (Exception ex)
            {
                //LogException(ex);
            }
        }

        public static void ProcessPacketBuffer(ITunnel tunnel, NtPacketBuffer packetBuffer, ProcessPacketCallback processPacketCallback)
        {
            try
            {
                if (packetBuffer.PacketBuilderLength + packetBuffer.SingleBufferUsedLength >= packetBuffer.PacketBuilder.Length)
                {
                    Array.Resize(ref packetBuffer.PacketBuilder, packetBuffer.PacketBuilderLength + packetBuffer.SingleBufferUsedLength);
                }

                Buffer.BlockCopy(packetBuffer.SingleBuffer, 0, packetBuffer.PacketBuilder, packetBuffer.PacketBuilderLength, packetBuffer.SingleBufferUsedLength);

                packetBuffer.PacketBuilderLength = packetBuffer.PacketBuilderLength + packetBuffer.SingleBufferUsedLength;

                while (packetBuffer.PacketBuilderLength > Sanity.PACKET_HEADER_SIZE) //[PacketSize] and [CRC16]
                {
                    var packetDelimiterBytes = new byte[4];
                    var packetSizeBytes = new byte[4];
                    var expectedCRC16Bytes = new byte[2];

                    Buffer.BlockCopy(packetBuffer.PacketBuilder, 0, packetDelimiterBytes, 0, packetDelimiterBytes.Length);
                    Buffer.BlockCopy(packetBuffer.PacketBuilder, 4, packetSizeBytes, 0, packetSizeBytes.Length);
                    Buffer.BlockCopy(packetBuffer.PacketBuilder, 8, expectedCRC16Bytes, 0, expectedCRC16Bytes.Length);

                    var packetDelimiter = BitConverter.ToInt32(packetDelimiterBytes, 0);
                    var grossPacketSize = BitConverter.ToInt32(packetSizeBytes, 0);
                    var expectedCRC16 = BitConverter.ToUInt16(expectedCRC16Bytes, 0);

                    if (packetDelimiter != Sanity.PACKET_DELIMITER)
                    {
                        //LogException(new Exception("Malformed packet, invalid delimiter."));
                        SkipPacket(ref packetBuffer);
                        continue;
                    }

                    if (grossPacketSize < 0 || grossPacketSize > Sanity.PACKET_MAX_SIZE)
                    {
                        //LogException(new Exception("Malformed packet, invalid length."));
                        SkipPacket(ref packetBuffer);
                        continue;
                    }

                    if (packetBuffer.PacketBuilderLength < grossPacketSize)
                    {
                        //We have data in the buffer, but it's not enough to make up
                        //  the entire message so we will break and wait on more data.
                        break;
                    }

                    var actualCRC16 = CRC16.ComputeChecksum(packetBuffer.PacketBuilder, Sanity.PACKET_HEADER_SIZE, grossPacketSize - Sanity.PACKET_HEADER_SIZE);

                    if (actualCRC16 != expectedCRC16)
                    {
                        //LogException(new Exception("Malformed packet, invalid CRC."));
                        SkipPacket(ref packetBuffer);
                        continue;
                    }

                    var netPacketSize = grossPacketSize - Sanity.PACKET_HEADER_SIZE;
                    var packetBytes = new byte[netPacketSize];

                    Buffer.BlockCopy(packetBuffer.PacketBuilder, Sanity.PACKET_HEADER_SIZE, packetBytes, 0, netPacketSize);

                    var packetBody = Decompress(packetBytes);

                    var packet = ToObject<NtPacket>(packetBody);

                    processPacketCallback(tunnel, packet);

                    //Zero out the consumed portion of the packet buffer - more for fun than anything else.
                    Array.Clear(packetBuffer.PacketBuilder, 0, grossPacketSize);

                    Buffer.BlockCopy(packetBuffer.PacketBuilder, grossPacketSize, packetBuffer.PacketBuilder, 0, packetBuffer.PacketBuilderLength - grossPacketSize);
                    packetBuffer.PacketBuilderLength -= grossPacketSize;
                }
            }
            catch (Exception ex)
            {
                //LogException(ex);
            }
        }

        private static byte[]? ToByteArray(object obj)
        {
            if (obj == null) return null;

            using var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);
            return stream.ToArray();
        }

        private static T ToObject<T>(byte[] arrBytes)
        {
            using var stream = new MemoryStream();
            stream.Write(arrBytes, 0, arrBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize<T>(stream);
        }

        private static byte[] Compress(byte[]? bytes)
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

        private static byte[] Decompress(byte[] bytes)
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
