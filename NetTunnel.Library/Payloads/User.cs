namespace NetTunnel.Library.Payloads
{
    public class User
    {
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }

        public User(string username, string passwordHash)
        {
            Username = username.ToLower();
            PasswordHash = passwordHash.ToLower();
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash.ToLower();
        }

        public User Clone()
        {
            return new User(Username, "");
        }
    }
}
