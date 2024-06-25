using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryCreateUser : IRmQuery<UIQueryCreateUserReply>
    {
        public User User { get; set; }

        public UIQueryCreateUser(User user)
        {
            User = user;
        }
    }

    public class UIQueryCreateUserReply : IRmQueryReply
    {
        public UIQueryCreateUserReply()
        {
        }
    }
}
