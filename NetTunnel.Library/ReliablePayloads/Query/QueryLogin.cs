using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query
{
    public class QueryLogin : IRmQuery<QueryLoginReply>
    {
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public QueryLogin(string username, string passwordHash)
        {
            UserName = username;
            PasswordHash = passwordHash;
        }
    }

    public class QueryLoginReply : IRmQueryReply
    {
        public bool Successful { get; set; }
        public Guid? ServiceId { get; set; }

        public QueryLoginReply()
        {
        }

        public QueryLoginReply(bool successful)
        {
            Successful = successful;
        }
    }
}
