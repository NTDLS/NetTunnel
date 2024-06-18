namespace NetTunnel.Library.Types
{
    public class TunnelTag
    {
        public NtTunnelConfiguration Tunnel { get; set; }

        public TunnelTag(NtTunnelConfiguration tunnel)
        {
            Tunnel = tunnel;
        }
    }
}
