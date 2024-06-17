using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The endpoint contains information that defines all we need to listen or connect to an endpoint.
    /// They are OWNED by the tunnel configuration and at the server that created the tunnel.
    /// These are sent to the tunnel service when the tunnel is connected, but once the connection
    ///     is made - they can be altered at either end by the service UI.
    /// </summary>
    public class NtEndpointConfiguration
    {
        public Guid EndpointId { get; set; }
        public Guid TunnelId { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtDirection Direction { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OutboundAddress { get; set; } = string.Empty;
        public int InboundPort { get; set; }
        public int OutboundPort { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtTrafficType TrafficType { get; set; } = NtTrafficType.Raw;
        public List<NtHttpHeaderRule> HttpHeaderRules { get; set; } = new();

        public NtEndpointConfiguration()
        {
        }

        public NtEndpointConfiguration(Guid tunnelId, Guid endpointId, string name,
            string outboundAddress, int inboundPort, int outboundPort, List<NtHttpHeaderRule> httpHeaderRules, NtTrafficType trafficType)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Direction = NtDirection.Undefined;
            Name = name;
            OutboundAddress = outboundAddress;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            TrafficType = trafficType;
            HttpHeaderRules.AddRange(httpHeaderRules);
        }

        public NtEndpointConfiguration(Guid tunnelId, Guid endpointId, NtDirection direction, string name,
            string outboundAddress, int inboundPort, int outboundPort, List<NtHttpHeaderRule> httpHeaderRules, NtTrafficType trafficType)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Direction = direction;
            Name = name;
            OutboundAddress = outboundAddress;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            TrafficType = trafficType;
            HttpHeaderRules.AddRange(httpHeaderRules);
        }

        public NtEndpointConfiguration CloneConfiguration()
        {
            var clone = new NtEndpointConfiguration
            {
                TunnelId = TunnelId,
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
            return EndpointId.GetHashCode()
                + TunnelId.GetHashCode()
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
