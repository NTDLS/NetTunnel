using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    public class EndpointProperties
    {
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; set; }
        public ulong CurrentConnections { get; set; }
        public Guid EndpointId { get; set; }
        public bool KeepRunning { get; set; } = false;

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public DirectionalKey? TunnelKey { get; set; }
        public DirectionalKey? EndpointKey { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtTrafficType TrafficType { get; set; } = NtTrafficType.Raw;
        public string Name { get; set; } = string.Empty;
        public string OutboundAddress { get; set; } = string.Empty;
        public int InboundPort { get; set; }
        public int OutboundPort { get; set; }
        public int HttpHeaderRules { get; set; }
    }
}
