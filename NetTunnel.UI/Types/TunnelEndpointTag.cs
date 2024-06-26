using NetTunnel.Library.Payloads;
using NTDLS.Helpers;

namespace NetTunnel.UI.Types
{
    public class TunnelEndpointTag
    {
        public TunnelDisplay Tunnel { get; set; }
        public EndpointConfiguration Endpoint { get; set; }

        public TunnelEndpointTag(TunnelDisplay tunnel, EndpointConfiguration endpoint)
        {
            Tunnel = tunnel;
            Endpoint = endpoint;
        }

        public static TunnelEndpointTag FromItem(ListViewItem item)
            => (item.Tag as TunnelEndpointTag).EnsureNotNull();

        public static TunnelEndpointTag? FromItemOrDefault(ListViewItem? item)
            => (item?.Tag as TunnelEndpointTag);
    }
}
