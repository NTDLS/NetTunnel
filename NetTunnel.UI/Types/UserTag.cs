using NetTunnel.Library.Payloads;
using NTDLS.Helpers;

namespace NetTunnel.UI.Types
{
    public class UserTag
    {
        public User User { get; set; }

        public UserTag(User user)
        {
            User = user;
        }

        public static UserTag FromItem(ListViewItem item)
            => (item.Tag as UserTag).EnsureNotNull();

        public static UserTag? FromItemOrDefault(ListViewItem? item)
            => (item?.Tag as UserTag);
    }
}
