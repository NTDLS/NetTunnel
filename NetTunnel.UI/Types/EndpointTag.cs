using NetTunnel.Library.Payloads;
using NTDLS.Helpers;

namespace NetTunnel.UI.Types
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

        public static EndpointTag FromItem(ListViewItem item)
            => (item.Tag as EndpointTag).EnsureNotNull();

        public static EndpointTag? FromItemOrDefault(ListViewItem? item)
            => (item?.Tag as EndpointTag);
    }
}
