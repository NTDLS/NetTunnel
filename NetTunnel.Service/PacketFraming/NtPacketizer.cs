using NetTunnel.Library;
using NetTunnel.Service.PacketFraming.PacketPayloads;
using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.PacketFraming.PacketPayloads.Queries;
using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;
using NetTunnel.Service.Types;
using NTDLS.Semaphore;
using System.Net.Sockets;
using System.Reflection;
using static NetTunnel.Service.PacketFraming.Types;

namespace NetTunnel.Service.PacketFraming
{
    /// <summary>
    /// TCP packets can be fragmented or combined. The packetizer rebuilds what was originally
    /// sent via the TCP send() call, provides compression and also performs a CRC check.
    /// </summary>
    internal static class NtPacketizer
    {
        private static CriticalResource<Dictionary<string, MethodInfo>> _reflectioncache = new();


        public static byte[] AssemblePacket(ITunnel tunnel, NtPacket packet)
        {
            try
            {
                var packetBody = Utility.SerializeToByteArray(packet);
                var packetBytes = Utility.Compress(packetBody);
                var grossPacketSize = packetBytes.Length + NtPacketDefaults.PACKET_HEADER_SIZE;
                var grossPacketBytes = new byte[grossPacketSize];
                var packetCrc = CRC16.ComputeChecksum(packetBytes);

                Buffer.BlockCopy(BitConverter.GetBytes(NtPacketDefaults.PACKET_DELIMITER), 0, grossPacketBytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(grossPacketSize), 0, grossPacketBytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(packetCrc), 0, grossPacketBytes, 8, 2);
                Buffer.BlockCopy(packetBytes, 0, grossPacketBytes, NtPacketDefaults.PACKET_HEADER_SIZE, packetBytes.Length);

                return grossPacketBytes;
            }
            catch (Exception ex)
            {
                tunnel.Core.Logging.Write(ex.Message);
                throw;
            }
        }

        private static void SkipPacket(ITunnel tunnel, ref NtPacketBuffer packetBuffer)
        {
            try
            {
                var packetDelimiterBytes = new byte[4];

                for (int offset = 1; offset < packetBuffer.PacketBuilderLength - packetDelimiterBytes.Length; offset++)
                {
                    Buffer.BlockCopy(packetBuffer.PacketBuilder, offset, packetDelimiterBytes, 0, packetDelimiterBytes.Length);

                    var value = BitConverter.ToInt32(packetDelimiterBytes, 0);

                    if (value == NtPacketDefaults.PACKET_DELIMITER)
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
                tunnel.Core.Logging.Write(ex.Message);
            }
        }

        public static void ReceiveAndProcessStreamPackets(NetworkStream? stream,
            ITunnel tunnel, NtPacketBuffer packetBuffer, ProcessPacketNotification
            processNotificationCallback, ProcessPacketQuery processPacketQueryCallback)
        {
            if (stream == null)
            {
                throw new Exception("ReceiveAndProcessStreamPackets stream can not be null.");
            }

            Array.Clear(packetBuffer.ReceiveBuffer);
            packetBuffer.ReceiveBufferUsed = stream.Read(packetBuffer.ReceiveBuffer, 0, packetBuffer.ReceiveBuffer.Length);
            ProcessPacketBuffer(stream, tunnel, packetBuffer, processNotificationCallback, processPacketQueryCallback);
        }

        public static void ProcessPacketBuffer(NetworkStream? stream, ITunnel tunnel, NtPacketBuffer packetBuffer, ProcessPacketNotification processNotificationCallback,
             ProcessPacketQuery processPacketQueryCallback)
        {
            if (stream == null)
            {
                throw new Exception("ReceiveAndProcessStreamPackets stream can not be null.");
            }

            try
            {
                if (packetBuffer.PacketBuilderLength + packetBuffer.ReceiveBufferUsed >= packetBuffer.PacketBuilder.Length)
                {
                    Array.Resize(ref packetBuffer.PacketBuilder, packetBuffer.PacketBuilderLength + packetBuffer.ReceiveBufferUsed);
                }

                Buffer.BlockCopy(packetBuffer.ReceiveBuffer, 0, packetBuffer.PacketBuilder, packetBuffer.PacketBuilderLength, packetBuffer.ReceiveBufferUsed);

                packetBuffer.PacketBuilderLength = packetBuffer.PacketBuilderLength + packetBuffer.ReceiveBufferUsed;

                while (packetBuffer.PacketBuilderLength > NtPacketDefaults.PACKET_HEADER_SIZE) //[PacketSize] and [CRC16]
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

                    if (packetDelimiter != NtPacketDefaults.PACKET_DELIMITER)
                    {
                        //LogException(new Exception("Malformed packet, invalid delimiter."));
                        SkipPacket(tunnel, ref packetBuffer);
                        continue;
                    }

                    if (grossPacketSize < 0 || grossPacketSize > NtPacketDefaults.PACKET_MAX_SIZE)
                    {
                        //LogException(new Exception("Malformed packet, invalid length."));
                        SkipPacket(tunnel, ref packetBuffer);
                        continue;
                    }

                    if (packetBuffer.PacketBuilderLength < grossPacketSize)
                    {
                        //We have data in the buffer, but it's not enough to make up
                        //  the entire message so we will break and wait on more data.
                        break;
                    }

                    var actualCRC16 = CRC16.ComputeChecksum(packetBuffer.PacketBuilder, NtPacketDefaults.PACKET_HEADER_SIZE, grossPacketSize - NtPacketDefaults.PACKET_HEADER_SIZE);

                    if (actualCRC16 != expectedCRC16)
                    {
                        //LogException(new Exception("Malformed packet, invalid CRC."));
                        SkipPacket(tunnel, ref packetBuffer);
                        continue;
                    }

                    var netPacketSize = grossPacketSize - NtPacketDefaults.PACKET_HEADER_SIZE;
                    var packetBytes = new byte[netPacketSize];

                    Buffer.BlockCopy(packetBuffer.PacketBuilder, NtPacketDefaults.PACKET_HEADER_SIZE, packetBytes, 0, netPacketSize);

                    var packetBody = Utility.Decompress(packetBytes);

                    var packet = Utility.DeserializeToObject<NtPacket>(packetBody);

                    //Zero out the consumed portion of the packet buffer - more for fun than anything else.
                    Array.Clear(packetBuffer.PacketBuilder, 0, grossPacketSize);

                    Buffer.BlockCopy(packetBuffer.PacketBuilder, grossPacketSize, packetBuffer.PacketBuilder, 0, packetBuffer.PacketBuilderLength - grossPacketSize);
                    packetBuffer.PacketBuilderLength -= grossPacketSize;

                    var payload = ExtractPacketPayload(packet);

                    if (payload is IPacketPayloadQuery query)
                    {
                        var replyPayload = processPacketQueryCallback(tunnel, query);
                        tunnel.SendStreamPacketPayloadReply(packet, replyPayload);
                    }
                    else if (payload is IPacketPayloadReply reply)
                    {
                        tunnel.ApplyQueryReply(packet.Id, reply);
                    }
                    else if (payload is IPacketPayloadNotification notification)
                    {
                        processNotificationCallback(tunnel, notification);
                    }
                    else
                    {
                        throw new Exception("Encountered undefined packet payload type.");
                    }
                }
            }
            catch (Exception ex)
            {
                tunnel.Core.Logging.Write(ex.Message);
            }
        }

        internal static IPacketPayload ExtractPacketPayload(NtPacket packet)
        {
            var genericToObjectMethod = _reflectioncache.Use((o) =>
            {
                if (o.TryGetValue(packet.EnclosedPayloadType, out var method))
                {
                    return method;
                }
                return null;
            });

            if (genericToObjectMethod != null)
            {
                return (IPacketPayload?)genericToObjectMethod.Invoke(null, new object[] { packet.Payload })
                    ?? throw new Exception($"Payload can not be null.");
            }

            var genericType = Type.GetType(packet.EnclosedPayloadType)
                ?? throw new Exception($"Unknown payload type {packet.EnclosedPayloadType}.");

            var toObjectMethod = typeof(Utility).GetMethod("DeserializeToObject")
                ?? throw new Exception($"Could not find ToObject().");

            genericToObjectMethod = toObjectMethod.MakeGenericMethod(genericType);

            _reflectioncache.Use((o) => o.TryAdd(packet.EnclosedPayloadType, genericToObjectMethod));

            return (IPacketPayload?)genericToObjectMethod.Invoke(null, new object[] { packet.Payload })
                ?? throw new Exception($"Payload can not be null.");
        }
    }
}
