using System;

namespace NetTunnel.Library.Tunneling
{
    [Serializable]
    public class PacketEnvelope
    {
        public DateTime CreatedTime = DateTime.UtcNow;
        public string Label { get; set; }
        public byte[] Payload { get; set; }
    }
}