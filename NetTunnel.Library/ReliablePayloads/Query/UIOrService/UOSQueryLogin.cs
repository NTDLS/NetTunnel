using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Query.UIOrService
{
    public class UOSQueryLogin : IRmQuery<UOSQueryLoginReply>
    {
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public UOSQueryLogin(string username, string passwordHash)
        {
            UserName = username;
            PasswordHash = passwordHash;
        }
    }

    public class UOSQueryLoginReply : IRmQueryReply
    {
        public bool Successful { get; set; }
        public Guid? ServiceId { get; set; }

        public UOSQueryLoginReply()
        {
        }

        public UOSQueryLoginReply(bool successful)
        {
            Successful = successful;
        }
    }
}
