using ProtoBuf;
using System.IO.Compression;
using static NetTunnel.Service.TCPIP.Constants;

namespace NetTunnel.Service.TCPIP
{
    /// <summary>
    /// TCP packets can be fragmented or combined. The packetizer rebuilds what was originally
    /// sent via the TCP send() call, provides compression and also performs a CRC check.
    /// </summary>
    internal static class Packetizer
    {
        public delegate void ProcessPayloadCallback(NtPeer peer, NtCommand payload);

        public static byte[] AssembleMessagePacket(NtCommand payload)
        {
            try
            {
                var payloadBody = ToByteArray(payload);
                var payloadBytes = Compress(payloadBody);
                var grossPacketSize = payloadBytes.Length + Sanity.PACKET_HEADER_SIZE;
                var packetBytes = new byte[grossPacketSize];
                var payloadCrc = CRC16.ComputeChecksum(payloadBytes);

                Buffer.BlockCopy(BitConverter.GetBytes(Sanity.PACKET_DELIMITER), 0, packetBytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(grossPacketSize), 0, packetBytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(payloadCrc), 0, packetBytes, 8, 2);
                Buffer.BlockCopy(payloadBytes, 0, packetBytes, Sanity.PACKET_HEADER_SIZE, payloadBytes.Length);

                return packetBytes;
            }
            catch (Exception ex)
            {
                //LogException(ex);
                throw;
            }
        }

        private static void SkipPacket(ref NtPacket packet)
        {
            try
            {
                var payloadDelimiterBytes = new byte[4];

                for (int offset = 1; offset < packet.PayloadBuilderLength - payloadDelimiterBytes.Length; offset++)
                {
                    Buffer.BlockCopy(packet.PayloadBuilder, offset, payloadDelimiterBytes, 0, payloadDelimiterBytes.Length);

                    var value = BitConverter.ToInt32(payloadDelimiterBytes, 0);

                    if (value == Sanity.PACKET_DELIMITER)
                    {
                        Buffer.BlockCopy(packet.PayloadBuilder, offset, packet.PayloadBuilder, 0, packet.PayloadBuilderLength - offset);
                        packet.PayloadBuilderLength -= offset;
                        return;
                    }
                }
                Array.Clear(packet.PayloadBuilder, 0, packet.PayloadBuilder.Length);
                packet.PayloadBuilderLength = 0;
            }
            catch (Exception ex)
            {
                //LogException(ex);
            }
        }

        public static void DissasemblePacketData(NtPeer peer, NtPacket packet, ProcessPayloadCallback processPayload)
        {
            try
            {
                if (packet.PayloadBuilderLength + packet.BufferLength >= packet.PayloadBuilder.Length)
                {
                    Array.Resize(ref packet.PayloadBuilder, packet.PayloadBuilderLength + packet.BufferLength);
                }

                Buffer.BlockCopy(packet.Buffer, 0, packet.PayloadBuilder, packet.PayloadBuilderLength, packet.BufferLength);

                packet.PayloadBuilderLength = packet.PayloadBuilderLength + packet.BufferLength;

                while (packet.PayloadBuilderLength > Sanity.PACKET_HEADER_SIZE) //[PayloadSize] and [CRC16]
                {
                    var payloadDelimiterBytes = new byte[4];
                    var payloadSizeBytes = new byte[4];
                    var expectedCRC16Bytes = new byte[2];

                    Buffer.BlockCopy(packet.PayloadBuilder, 0, payloadDelimiterBytes, 0, payloadDelimiterBytes.Length);
                    Buffer.BlockCopy(packet.PayloadBuilder, 4, payloadSizeBytes, 0, payloadSizeBytes.Length);
                    Buffer.BlockCopy(packet.PayloadBuilder, 8, expectedCRC16Bytes, 0, expectedCRC16Bytes.Length);

                    var payloadDelimiter = BitConverter.ToInt32(payloadDelimiterBytes, 0);
                    var grossPayloadSize = BitConverter.ToInt32(payloadSizeBytes, 0);
                    var expectedCRC16 = BitConverter.ToUInt16(expectedCRC16Bytes, 0);

                    if (payloadDelimiter != Sanity.PACKET_DELIMITER)
                    {
                        //LogException(new Exception("Malformed payload packet, invalid delimiter."));
                        SkipPacket(ref packet);
                        continue;
                    }

                    if (grossPayloadSize < 0 || grossPayloadSize > Sanity.PACKET_MAX_SIZE)
                    {
                        //LogException(new Exception("Malformed payload packet, invalid length."));
                        SkipPacket(ref packet);
                        continue;
                    }

                    if (packet.PayloadBuilderLength < grossPayloadSize)
                    {
                        //We have data in the buffer, but it's not enough to make up
                        //  the entire message so we will break and wait on more data.
                        break;
                    }

                    var actualCRC16 = CRC16.ComputeChecksum(packet.PayloadBuilder, Sanity.PACKET_HEADER_SIZE, grossPayloadSize - Sanity.PACKET_HEADER_SIZE);

                    if (actualCRC16 != expectedCRC16)
                    {
                        //LogException(new Exception("Malformed payload packet, invalid CRC."));
                        SkipPacket(ref packet);
                        continue;
                    }

                    var netPayloadSize = grossPayloadSize - Sanity.PACKET_HEADER_SIZE;
                    var payloadBytes = new byte[netPayloadSize];

                    Buffer.BlockCopy(packet.PayloadBuilder, Sanity.PACKET_HEADER_SIZE, payloadBytes, 0, netPayloadSize);

                    var payloadBody = Decompress(payloadBytes);

                    var payload = ToObject<NtCommand>(payloadBody);

                    processPayload(peer, payload);

                    //Zero out the consumed portion of the payload buffer - more for fun than anything else.
                    Array.Clear(packet.PayloadBuilder, 0, grossPayloadSize);

                    Buffer.BlockCopy(packet.PayloadBuilder, grossPayloadSize, packet.PayloadBuilder, 0, packet.PayloadBuilderLength - grossPayloadSize);
                    packet.PayloadBuilderLength -= grossPayloadSize;
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
