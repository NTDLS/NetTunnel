using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryCreateUser : IRmQuery<QueryCreateUserReply>
    {
        public User User { get; set; }

        public QueryCreateUser(User user)
        {
            User = user;
        }
    }

    public class QueryCreateUserReply : IRmQueryReply
    {
        public QueryCreateUserReply()
        {
        }
    }
}
