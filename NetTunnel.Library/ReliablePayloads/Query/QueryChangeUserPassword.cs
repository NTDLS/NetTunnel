using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryChangeUserPassword : IRmQuery<QueryChangeUserPasswordReply>
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public QueryChangeUserPassword(string username, string passwordHash)
        {
            Username = username;
            PasswordHash = passwordHash;
        }
    }

    public class QueryChangeUserPasswordReply : IRmQueryReply
    {
        public QueryChangeUserPasswordReply()
        {
        }
    }
}
