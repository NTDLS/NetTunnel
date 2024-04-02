using NetTunnel.Library;
using NetTunnel.Service.MessageFraming.FramePayloads;
using NetTunnel.Service.MessageFraming.FramePayloads.Notifications;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.MessageFraming.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.Semaphore;
using System.Net.Sockets;
using System.Reflection;
using static NetTunnel.Service.MessageFraming.Types;

namespace NetTunnel.Service.MessageFraming
{
    /// <summary>
    /// TCP packets can be fragmented or combined. The packetizer rebuilds what was originally
    /// sent via the TCP send() call, provides compression and also performs a CRC check.
    /// </summary>
    internal static class NtFraming
    {
        private static readonly PessimisticCriticalResource<Dictionary<string, MethodInfo>> _reflectioncache = new();

        public static byte[] AssembleFrame(ITunnel tunnel, NtFrame frame)
        {
            try
            {
                var frameBody = Utility.SerializeToByteArray(frame);
                var frameBytes = Utility.Compress(frameBody);

                if (tunnel.SecureKeyExchangeIsComplete)
                {
                    tunnel.NascclStream?.Cipher(ref frameBytes);
                }
                var grossFrameSize = frameBytes.Length + NtFrameDefaults.FRAME_HEADER_SIZE;
                var grossFrameBytes = new byte[grossFrameSize];
                var frameCrc = CRC16.ComputeChecksum(frameBytes);

                Buffer.BlockCopy(BitConverter.GetBytes(NtFrameDefaults.FRAME_DELIMITER), 0, grossFrameBytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(grossFrameSize), 0, grossFrameBytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(frameCrc), 0, grossFrameBytes, 8, 2);
                Buffer.BlockCopy(frameBytes, 0, grossFrameBytes, NtFrameDefaults.FRAME_HEADER_SIZE, frameBytes.Length);

                return grossFrameBytes;
            }
            catch (Exception ex)
            {
                tunnel.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"AssembleFrame: {ex.Message}");
                throw;
            }
        }

        private static void SkipFrame(ITunnel tunnel, ref NtFrameBuffer frameBuffer)
        {
            try
            {
                var frameDelimiterBytes = new byte[4];

                for (int offset = 1; offset < frameBuffer.FrameBuilderLength - frameDelimiterBytes.Length; offset++)
                {
                    Buffer.BlockCopy(frameBuffer.FrameBuilder, offset, frameDelimiterBytes, 0, frameDelimiterBytes.Length);

                    var value = BitConverter.ToInt32(frameDelimiterBytes, 0);

                    if (value == NtFrameDefaults.FRAME_DELIMITER)
                    {
                        Buffer.BlockCopy(frameBuffer.FrameBuilder, offset, frameBuffer.FrameBuilder, 0, frameBuffer.FrameBuilderLength - offset);
                        frameBuffer.FrameBuilderLength -= offset;
                        return;
                    }
                }
                Array.Clear(frameBuffer.FrameBuilder, 0, frameBuffer.FrameBuilder.Length);
                frameBuffer.FrameBuilderLength = 0;
            }
            catch (Exception ex)
            {
                tunnel.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"SkipFrame: {ex.Message}");
            }
        }

        public static bool ReceiveAndProcessStreamFrames(NetworkStream? stream,
            ITunnel tunnel, NtFrameBuffer frameBuffer, ProcessFrameNotification
            processNotificationCallback, ProcessFrameQuery processFrameQueryCallback)
        {
            if (stream == null)
            {
                throw new Exception("ReceiveAndProcessStreamFrames: stream can not be null.");
            }

            Array.Clear(frameBuffer.ReceiveBuffer);
            frameBuffer.ReceiveBufferUsed = stream.Read(frameBuffer.ReceiveBuffer, 0, frameBuffer.ReceiveBuffer.Length);
            if (frameBuffer.ReceiveBufferUsed == 0)
            {
                return false;
            }

            tunnel.BytesReceived += (ulong)frameBuffer.ReceiveBufferUsed;

            ProcessFrameBuffer(stream, tunnel, frameBuffer, processNotificationCallback, processFrameQueryCallback);

            return true;
        }

        public static void ProcessFrameBuffer(NetworkStream? stream, ITunnel tunnel, NtFrameBuffer frameBuffer, ProcessFrameNotification processNotificationCallback,
             ProcessFrameQuery processFrameQueryCallback)
        {
            if (stream == null)
            {
                throw new Exception("ProcessFrameBuffer: stream can not be null.");
            }

            try
            {
                if (frameBuffer.FrameBuilderLength + frameBuffer.ReceiveBufferUsed >= frameBuffer.FrameBuilder.Length)
                {
                    Array.Resize(ref frameBuffer.FrameBuilder, frameBuffer.FrameBuilderLength + frameBuffer.ReceiveBufferUsed);
                }

                Buffer.BlockCopy(frameBuffer.ReceiveBuffer, 0, frameBuffer.FrameBuilder, frameBuffer.FrameBuilderLength, frameBuffer.ReceiveBufferUsed);

                frameBuffer.FrameBuilderLength = frameBuffer.FrameBuilderLength + frameBuffer.ReceiveBufferUsed;

                while (frameBuffer.FrameBuilderLength > NtFrameDefaults.FRAME_HEADER_SIZE) //[FrameSize] and [CRC16]
                {
                    var frameDelimiterBytes = new byte[4];
                    var frameSizeBytes = new byte[4];
                    var expectedCRC16Bytes = new byte[2];

                    Buffer.BlockCopy(frameBuffer.FrameBuilder, 0, frameDelimiterBytes, 0, frameDelimiterBytes.Length);
                    Buffer.BlockCopy(frameBuffer.FrameBuilder, 4, frameSizeBytes, 0, frameSizeBytes.Length);
                    Buffer.BlockCopy(frameBuffer.FrameBuilder, 8, expectedCRC16Bytes, 0, expectedCRC16Bytes.Length);

                    var frameDelimiter = BitConverter.ToInt32(frameDelimiterBytes, 0);
                    var grossFrameSize = BitConverter.ToInt32(frameSizeBytes, 0);
                    var expectedCRC16 = BitConverter.ToUInt16(expectedCRC16Bytes, 0);

                    if (frameDelimiter != NtFrameDefaults.FRAME_DELIMITER)
                    {
                        tunnel.Core.Logging.Write(Constants.NtLogSeverity.Warning, "ProcessFrameBuffer: Malformed frame, invalid delimiter.");
                        SkipFrame(tunnel, ref frameBuffer);
                        continue;
                    }

                    if (grossFrameSize < 0 || grossFrameSize > Singletons.Configuration.MaxFrameSize)
                    {
                        tunnel.Core.Logging.Write(Constants.NtLogSeverity.Warning, "ProcessFrameBuffer: Malformed frame, invalid length.");
                        SkipFrame(tunnel, ref frameBuffer);
                        continue;
                    }

                    if (frameBuffer.FrameBuilderLength < grossFrameSize)
                    {
                        //We have data in the buffer, but it's not enough to make up
                        //  the entire message so we will break and wait on more data.
                        break;
                    }

                    var actualCRC16 = CRC16.ComputeChecksum(frameBuffer.FrameBuilder, NtFrameDefaults.FRAME_HEADER_SIZE, grossFrameSize - NtFrameDefaults.FRAME_HEADER_SIZE);

                    if (actualCRC16 != expectedCRC16)
                    {
                        tunnel.Core.Logging.Write(Constants.NtLogSeverity.Warning, "ProcessFrameBuffer: Malformed frame, invalid CRC.");
                        SkipFrame(tunnel, ref frameBuffer);
                        continue;
                    }

                    var netFrameSize = grossFrameSize - NtFrameDefaults.FRAME_HEADER_SIZE;
                    var frameBytes = new byte[netFrameSize];
                    Buffer.BlockCopy(frameBuffer.FrameBuilder, NtFrameDefaults.FRAME_HEADER_SIZE, frameBytes, 0, netFrameSize);
                    if (tunnel.SecureKeyExchangeIsComplete)
                    {
                        tunnel.NascclStream?.Cipher(ref frameBytes);
                    }

                    var frameBody = Utility.Decompress(frameBytes);
                    var frame = Utility.DeserializeToObject<NtFrame>(frameBody);

                    //Zero out the consumed portion of the frame buffer - more for fun than anything else.
                    Array.Clear(frameBuffer.FrameBuilder, 0, grossFrameSize);

                    Buffer.BlockCopy(frameBuffer.FrameBuilder, grossFrameSize, frameBuffer.FrameBuilder, 0, frameBuffer.FrameBuilderLength - grossFrameSize);
                    frameBuffer.FrameBuilderLength -= grossFrameSize;

                    var payload = ExtractFramePayload(frame);

                    if (payload is INtFramePayloadQuery query)
                    {
                        var replyPayload = processFrameQueryCallback(tunnel, query);
                        tunnel.SendStreamFramePayloadReply(frame, replyPayload);
                    }
                    else if (payload is INtFramePayloadReply reply)
                    {
                        tunnel.ApplyQueryReply(frame.Id, reply);
                    }
                    else if (payload is INtFramePayloadNotification notification)
                    {
                        processNotificationCallback(tunnel, notification);
                    }
                    else
                    {
                        throw new Exception("ProcessFrameBuffer: Encountered undefined frame payload type.");
                    }
                }
            }
            catch (Exception ex)
            {
                tunnel.Core.Logging.Write(Constants.NtLogSeverity.Exception, $"ProcessFrameBuffer: {ex.Message}");
            }
        }

        internal static INtFramePayload ExtractFramePayload(NtFrame frame)
        {
            var genericToObjectMethod = _reflectioncache.Use((o) =>
            {
                if (o.TryGetValue(frame.EnclosedPayloadType, out var method))
                {
                    return method;
                }
                return null;
            });

            if (genericToObjectMethod != null)
            {
                return (INtFramePayload?)genericToObjectMethod.Invoke(null, new object[] { frame.Payload })
                    ?? throw new Exception($"ExtractFramePayload: Payload can not be null.");
            }

            var genericType = Type.GetType(frame.EnclosedPayloadType)
                ?? throw new Exception($"ExtractFramePayload: Unknown payload type {frame.EnclosedPayloadType}.");

            var toObjectMethod = typeof(Utility).GetMethod("DeserializeToObject")
                ?? throw new Exception($"ExtractFramePayload: Could not find ToObject().");

            genericToObjectMethod = toObjectMethod.MakeGenericMethod(genericType);

            _reflectioncache.Use((o) => o.TryAdd(frame.EnclosedPayloadType, genericToObjectMethod));

            return (INtFramePayload?)genericToObjectMethod.Invoke(null, new object[] { frame.Payload })
                ?? throw new Exception($"ExtractFramePayload: Payload can not be null.");
        }
    }
}
