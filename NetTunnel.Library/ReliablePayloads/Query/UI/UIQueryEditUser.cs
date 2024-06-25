using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryEditUser : IRmQuery<UIQueryEditUserReply>
    {
        public User User { get; set; } = new();

        public UIQueryEditUser(User user)
        {
            User = user;
        }

        public UIQueryEditUser()
        {
        }
    }

    public class UIQueryEditUserReply : IRmQueryReply
    {
        public UIQueryEditUserReply()
        {
        }
    }
}
