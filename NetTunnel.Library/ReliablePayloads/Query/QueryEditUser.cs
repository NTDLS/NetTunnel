using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryEditUser : IRmQuery<QueryEditUserReply>
    {
        public User User { get; set; } = new();

        public QueryEditUser(User user)
        {
            User = user;
        }

        public QueryEditUser()
        {
        }
    }

    public class QueryEditUserReply : IRmQueryReply
    {
        public QueryEditUserReply()
        {
        }
    }
}
