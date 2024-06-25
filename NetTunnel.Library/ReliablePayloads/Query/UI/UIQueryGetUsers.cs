using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UI
{
    public class UIQueryGetUsers : IRmQuery<UIQueryGetUsersReply>
    {
    }

    public class UIQueryGetUsersReply : IRmQueryReply
    {
        public List<User> Collection { get; set; } = new();

        public UIQueryGetUsersReply()
        {
        }
    }
}
