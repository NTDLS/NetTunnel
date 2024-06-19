namespace NetTunnel.Library.Types
{
    public class EndpointTag
    {
        public TunnelDisplay Tunnel { get; set; }
        public EndpointDisplay Endpoint { get; set; }

        public EndpointTag(TunnelDisplay tunnel, EndpointDisplay endpoint)
        {
            Tunnel = tunnel;
            Endpoint = endpoint;
        }
    }
}
