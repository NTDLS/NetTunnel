using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    /// <summary>
    /// The inbound endpoint contains information that defines an inbound/listening connection for an established endpoint.
    /// </summary>
    public class NtEndpointInboundConfiguration : INtEndpointConfiguration
    {
        public Guid EndpointId { get; set; }
        public Guid TunnelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OutboundAddress { get; set; } = string.Empty;
        public int InboundPort { get; set; }
        public int OutboundPort { get; set; }
        public NtTrafficType TrafficType { get; set; } = NtTrafficType.Raw;
        public List<NtHttpHeaderRule> HttpHeaderRules { get; set; } = new();

        public NtEndpointInboundConfiguration()
        {
        }

        public NtEndpointInboundConfiguration(Guid tunnelId, Guid endpointId, string name,
            string outboundAddress, int inboundPort, int outboundPort, List<NtHttpHeaderRule> httpHeaderRules, NtTrafficType trafficType)
        {
            TunnelId = tunnelId;
            EndpointId = endpointId;
            Name = name;
            OutboundAddress = outboundAddress;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            TrafficType = trafficType;
            httpHeaderRules.AddRange(httpHeaderRules);
        }
    }
}
