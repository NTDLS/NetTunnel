using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryGetUsers : IRmQuery<QueryGetUsersReply>
    {
    }

    public class QueryGetUsersReply : IRmQueryReply
    {
        public List<User> Collection { get; set; } = new();

        public QueryGetUsersReply()
        {
        }
    }
}
