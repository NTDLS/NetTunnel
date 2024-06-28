using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.ReliablePayloads.Query.UIOrService
{
    public class UOSQueryLogin : IRmQuery<UOSQueryLoginReply>
    {
        public NtLoginType LoginType { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public UOSQueryLogin(string username, string passwordHash, NtLoginType loginType)
        {
            UserName = username;
            PasswordHash = passwordHash;
            LoginType = loginType;
        }
    }

    public class UOSQueryLoginReply : IRmQueryReply
    {
        public bool Successful { get; set; }
        public Guid? ServiceId { get; set; }
        public NtUserRole UserRole { get; set; } = NtUserRole.Undefined;

        public UOSQueryLoginReply()
        {
        }

        public UOSQueryLoginReply(bool successful)
        {
            Successful = successful;
        }
    }
}
