using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryDeleteUser : IRmQuery<UIQueryDeleteUserReply>
    {
        public string UserName { get; set; } = string.Empty;

        public UIQueryDeleteUser(string username)
        {
            UserName = username;
        }
    }

    public class UIQueryDeleteUserReply : IRmQueryReply
    {
        public UIQueryDeleteUserReply()
        {
        }
    }
}
