using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public interface INtEndpointConfiguration
    {
        public Guid EndpointId { get; }
        public Guid TunnelId { get; }
        public string Name { get; }
        public string OutboundAddress { get; }
        public int InboundPort { get; }
        public int OutboundPort { get; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtTrafficType TrafficType { get; }
        public List<NtHttpHeaderRule> HttpHeaderRules { get; }
    }
}
