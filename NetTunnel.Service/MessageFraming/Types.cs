using NetTunnel.Service.MessageFraming.FramePayloads.Notifications;
using NetTunnel.Service.MessageFraming.FramePayloads.Queries;
using NetTunnel.Service.MessageFraming.FramePayloads.Replies;
using NetTunnel.Service.TunnelEngine.Tunnels;

namespace NetTunnel.Service.MessageFraming
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
