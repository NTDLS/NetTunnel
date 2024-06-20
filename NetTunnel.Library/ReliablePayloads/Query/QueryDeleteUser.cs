using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryDeleteUser : IRmQuery<QueryDeleteUserReply>
    {
        public string UserName { get; set; } = string.Empty;

        public QueryDeleteUser(string username)
        {
            UserName = username;
        }
    }

    public class QueryDeleteUserReply : IRmQueryReply
    {
        public QueryDeleteUserReply()
        {
        }
    }
}
