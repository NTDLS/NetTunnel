namespace NetTunnel.Library.Types
{
    public class EndpointTag
    {
        public NtTunnelConfiguration Tunnel { get; set; }
        public NtEndpointConfiguration Endpoint { get; set; }

        public EndpointTag(NtTunnelConfiguration tunnel, NtEndpointConfiguration endpoint)
        {
            Tunnel = tunnel;
            Endpoint = endpoint;
        }
    }
}
