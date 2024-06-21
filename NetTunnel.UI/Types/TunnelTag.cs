using NetTunnel.Library.Payloads;
using NTDLS.Helpers;

namespace NetTunnel.UI.Types
{
    public class TunnelTag
    {
        public TunnelDisplay Tunnel { get; set; }

        public TunnelTag(TunnelDisplay tunnel)
        {
            Tunnel = tunnel;
        }

        public static TunnelTag FromItem(ListViewItem item)
            => (item.Tag as TunnelTag).EnsureNotNull();

        public static TunnelTag? FromItemOrDefault(ListViewItem? item)
            => (item?.Tag as TunnelTag);
    }
}
