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
            public const int FRAME_MAX_SIZE = 134217728; //128MB, resize all you want - its just a sanity check - not a hard limit.
            public const int FRAME_BUFFER_SIZE = 8192;
            public const int QUERY_TIMEOUT_MS = 60000;
            //public const int ACK_TIMEOUT_MS = 5000;
        }
    }
}
