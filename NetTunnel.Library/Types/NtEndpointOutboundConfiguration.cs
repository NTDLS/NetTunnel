using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The tunnel connector contains information that defines an outbound termination connection from an established endpoint.
    /// </summary>
    public class NtEndpointOutboundConfiguration : INtEndpointConfiguration
    {
        public Guid EndpointId { get; set; }
        public Guid TunnelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OutboundAddress { get; set; } = string.Empty;
        public int InboundPort { get; set; }
        public int OutboundPort { get; set; }
        public NtTrafficType TrafficType { get; set; } = NtTrafficType.Raw;
        public List<NtHttpHeaderRule> HttpHeaderRules { get; set; } = new();

        public NtEndpointOutboundConfiguration()
        {
        }

        public NtEndpointOutboundConfiguration(Guid tunnelId, Guid endpointId, string name,
            string address, int inboundPort, int outboundPort, List<NtHttpHeaderRule> httpHeaderRules, NtTrafficType trafficType)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Name = name;
            OutboundAddress = address;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            TrafficType = trafficType;
            HttpHeaderRules.AddRange(httpHeaderRules);
        }

        public override int GetHashCode()
        {
            return EndpointId.GetHashCode()
                + TunnelId.GetHashCode()
                + Name.GetHashCode()
                + OutboundAddress.GetHashCode()
                + InboundPort.GetHashCode()
                + OutboundPort.GetHashCode()
                + TrafficType.GetHashCode()
                + HttpHeaderRules.GetHashCode();
        }
    }
}
