using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    /// <summary>
    /// The endpoint contains information that defines all we need to listen or connect to an endpoint.
    /// They are OWNED by the tunnel configuration and at the server that created the tunnel.
    /// These are sent to the tunnel service when the tunnel is connected, but once the connection
    ///     is made - they can be altered at either end by the service UI.
    /// </summary>
    public class EndpointConfiguration
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
        public DirectionalKey EndpointKey => new(EndpointId, Direction);

        public EndpointConfiguration SwapDirection()
        {
            var clone = CloneConfiguration();
            clone.Direction = Constants.SwapDirection(clone.Direction);
            return clone;
        }

        public EndpointConfiguration()
        {
        }

        public EndpointConfiguration(Guid endpointId, string name,
            string outboundAddress, int inboundPort, int outboundPort, List<HttpHeaderRule> httpHeaderRules, NtTrafficType trafficType)
        {
            EndpointId = endpointId;
            Direction = NtDirection.Undefined;
            Name = name;
            OutboundAddress = outboundAddress;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            TrafficType = trafficType;
            HttpHeaderRules.AddRange(httpHeaderRules);
        }

        public EndpointConfiguration(Guid endpointId, NtDirection direction, string name,
            string outboundAddress, int inboundPort, int outboundPort, List<HttpHeaderRule> httpHeaderRules, NtTrafficType trafficType)
        {
            EndpointId = endpointId;
            Direction = direction;
            Name = name;
            OutboundAddress = outboundAddress;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            TrafficType = trafficType;
            HttpHeaderRules.AddRange(httpHeaderRules);
        }

        public EndpointConfiguration CloneConfiguration()
        {
            var clone = new EndpointConfiguration
            {
                EndpointId = EndpointId,
                Direction = Direction,
                Name = Name,
                OutboundAddress = OutboundAddress,
                InboundPort = InboundPort,
                OutboundPort = OutboundPort,
                TrafficType = TrafficType,
            };

            foreach (var rule in HttpHeaderRules)
            {
                clone.HttpHeaderRules.Add(rule.CloneConfiguration());
            }

            return clone;
        }

        public override int GetHashCode()
        {
            return Utility.CombineHashes([EndpointId.GetHashCode(),
                Name.GetHashCode(),
                Direction.GetHashCode(),
                OutboundAddress.GetHashCode(),
                InboundPort.GetHashCode(),
                OutboundPort.GetHashCode(),
                TrafficType.GetHashCode(),
                HttpHeaderRules.GetHashCode()]);
        }
    }
}
