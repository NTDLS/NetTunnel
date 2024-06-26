using NetTunnel.Library.Payloads;
using NTDLS.Helpers;

namespace NetTunnel.UI.Types
{
    public class EndpointTag
    {
        public EndpointConfiguration Endpoint { get; set; }

        public EndpointTag(EndpointConfiguration endpoint)
        {
            Endpoint = endpoint;
        }

        public static EndpointTag FromItem(ListViewItem item)
            => (item.Tag as EndpointTag).EnsureNotNull();

        public static EndpointTag? FromItemOrDefault(ListViewItem? item)
            => (item?.Tag as EndpointTag);
    }
}
