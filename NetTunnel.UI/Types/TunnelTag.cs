using NetTunnel.Library.Payloads;
using NTDLS.Helpers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.UI.Types
{
    public class TunnelTag
    {
        public TunnelDisplay Tunnel { get; set; }

        public NtTunnelStatus TunnelStatus { get; set; }

        public TunnelTag(TunnelDisplay tunnel)
        {
            Tunnel = tunnel;
        }

        public static TunnelTag FromItem(ListViewItem item)
        {
            var tag = (item.Tag as TunnelTag).EnsureNotNull();
            var map = new ListViewColumnMap(item.ListView);

            if (Enum.TryParse<NtTunnelStatus>(map.SubItem(item, "Status").Text, true, out var result))
            {
                tag.TunnelStatus = result;
            }
            else
            {
                tag.TunnelStatus = NtTunnelStatus.Undefined;
            }

            return tag;
        }

        public static TunnelTag? FromItemOrDefault(ListViewItem? item)
        {
            var tag = (item?.Tag as TunnelTag);

            if (tag != null && item != null)
            {
                var map = new ListViewColumnMap(item.ListView);

                if (Enum.TryParse<NtTunnelStatus>(map.SubItem(item, "Status").Text, true, out var result))
                {
                    tag.TunnelStatus = result;
                }
                else
                {
                    tag.TunnelStatus = NtTunnelStatus.Undefined;
                }
            }

            return tag;
        }
    }
}
