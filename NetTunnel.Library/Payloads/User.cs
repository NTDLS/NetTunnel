using Newtonsoft.Json.Converters;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Payloads
{
    public class User
    {
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// A NULL password means that the password hash was not supplied.
        /// </summary>
        public string? PasswordHash { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public NtUserRole Role { get; set; } = NtUserRole.Undefined;

        public List<EndpointConfiguration> Endpoints { get; set; } = new();

        public User(string username, string? passwordHash, NtUserRole role)
        {
            Username = username.ToLower();
            PasswordHash = passwordHash?.ToLower();
            Role = role;
        }

        public User()
        {
        }

        public void Modify(User user)
        {
            if (user.PasswordHash != null)
            {
                PasswordHash = user.PasswordHash;
            }
            Role = user.Role;
        }

        public User Clone()
        {
            return new User(Username, null, Role);
        }
    }
}
