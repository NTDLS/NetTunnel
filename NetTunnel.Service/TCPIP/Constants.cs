namespace NetTunnel.Service.TCPIP
{
    internal class Constants
    {
        internal static class Sanity
        {
            public const int PACKET_DELIMITER = 122455788;
            public const int PACKET_HEADER_SIZE = 10;
            public const int PACKET_MAX_SIZE = 1024 * 1024 * 128; //128MB, resize all you want - its just a sanity check - not a hard limit.
            public const int PACKET_BUFFER_SIZE = 1024 * 8;
            public const int ACK_TIMEOUT_MS = 5000;
        }

        internal enum PayloadCommandType
        {
            Unspecified,
            Hello,
            CommandAck
        }
    }
}
