using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public class EndpointDisplay
    {
        public Guid EndpointId { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OutboundAddress { get; set; } = string.Empty;
        public int InboundPort { get; set; }
        public int OutboundPort { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtTrafficType TrafficType { get; set; } = NtTrafficType.Raw;
        public List<HttpHeaderRule> HttpHeaderRules { get; set; } = new();

        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey EndpointKey => new DirectionalKey(EndpointId, Direction);

        public override int GetHashCode()
        {
            return EndpointId.GetHashCode()
                + Name.GetHashCode()
                + Direction.GetHashCode()
                + OutboundAddress.GetHashCode()
                + InboundPort.GetHashCode()
                + OutboundPort.GetHashCode()
                + TrafficType.GetHashCode()
                + HttpHeaderRules.GetHashCode();
        }
    }
}
