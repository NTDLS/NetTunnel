namespace NetTunnel.Library.Types
{
    public class TunnelTag
    {
        public TunnelConfiguration Tunnel { get; set; }

        public TunnelTag(TunnelConfiguration tunnel)
        {
            Tunnel = tunnel;
        }
    }
}
