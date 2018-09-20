using System;

namespace NetTunnel.Library.Routing
{
    [Serializable]
    public class PacketEnvelope
    {
        public DateTime CreatedTime = DateTime.UtcNow;
        public string Label { get; set; }
        public byte[] Payload { get; set; }
    }
}