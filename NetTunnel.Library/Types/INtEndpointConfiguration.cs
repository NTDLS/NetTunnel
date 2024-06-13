using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public interface INtEndpointConfiguration
    {
        public Guid EndpointId { get; }
        public Guid TunnelId { get; }
        public string Name { get; }
        public int TransmissionPort { get; }
        public NtTrafficType TrafficType { get; }
        public List<NtHttpHeaderRule> HttpHeaderRules { get; }
    }
}
