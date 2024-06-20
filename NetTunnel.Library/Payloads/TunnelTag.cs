namespace NetTunnel.Library.Payloads
{
    public class TunnelTag
    {
        public TunnelDisplay Tunnel { get; set; }

        public TunnelTag(TunnelDisplay tunnel)
        {
            Tunnel = tunnel;
        }
    }
}
