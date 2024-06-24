using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public NtUserRole Role { get; set; } = NtUserRole.Limited;

        public User(string username, string passwordHash, NtUserRole role)
        {
            Username = username.ToLower();
            PasswordHash = passwordHash.ToLower();
            Role = role;
        }

        public User()
        {
        }

        public void Modify(User user)
        {
            PasswordHash = user.PasswordHash;
            Role = user.Role;
        }

        public User Clone()
        {
            return new User(Username, "", Role);
        }
    }
}
