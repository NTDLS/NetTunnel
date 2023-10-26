using NetTunnel.Service.PacketFraming.PacketPayloads.Notifications;
using NetTunnel.Service.PacketFraming.PacketPayloads.Queries;
using NetTunnel.Service.PacketFraming.PacketPayloads.Replies;
using NetTunnel.Service.Types;

namespace NetTunnel.Service.PacketFraming
{
    internal class Types
    {
        public delegate void ProcessFrameNotification(ITunnel tunnel, INtFramePayloadNotification payload);

        public delegate INtFramePayloadReply ProcessFrameQuery(ITunnel tunnel, INtFramePayloadQuery payload);

        internal static class NtFrameDefaults
        {
            public const int FRAME_DELIMITER = 948724593;
            public const int FRAME_HEADER_SIZE = 10;
        }
    }
}
