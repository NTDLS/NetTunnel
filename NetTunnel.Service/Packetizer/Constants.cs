namespace NetTunnel.Service.Packetizer
{
    internal class Constants
    {
        internal static class NtPacketDefaults
        {
            public const int PACKET_DELIMITER = 948724593;
            public const int PACKET_HEADER_SIZE = 10;
            public const int PACKET_MAX_SIZE = 134217728; //128MB, resize all you want - its just a sanity check - not a hard limit.
            public const int PACKET_BUFFER_SIZE = 8192;
            //public const int ACK_TIMEOUT_MS = 5000;
        }
    }
}
