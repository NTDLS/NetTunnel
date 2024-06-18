namespace NetTunnel.Library.Types
{
    public class EndpointTag
    {
        public TunnelConfiguration Tunnel { get; set; }
        public EndpointConfiguration Endpoint { get; set; }

        public EndpointTag(TunnelConfiguration tunnel, EndpointConfiguration endpoint)
        {
            Tunnel = tunnel;
            Endpoint = endpoint;
        }
    }
}
