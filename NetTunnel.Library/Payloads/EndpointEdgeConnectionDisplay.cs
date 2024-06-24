﻿namespace NetTunnel.Library.Payloads
{
    public class EndpointEdgeConnectionDisplay
    {
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime LastActivityDateTime { get; set; }
        public Guid EdgeId { get; set; }
        public int Port { get; set; }
        public int ThreadId { get; set; }
        public bool IsConnected { get; set; }
        public string AddressFamily { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}