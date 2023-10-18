using NTDLS.Semaphore;

namespace NetTunnel.Engine.Managers
{
    public class UserManager
    {
        private readonly EngineCore _core;

        public CriticalResource<List<NtUser>> Collection { get; set; } = new();

        public UserManager(EngineCore core)
        {
            _core = core;
        }

        public void Add(string username, string passwordHash)
        {
            Add(new NtUser(username, passwordHash));
        }

        public void Add(NtUser user)
        {
            Collection.Use((o) => o.Add(user));
        }

        public bool ValidateLogin(string username, string passwordHash)
        {
            username = username.ToLower();
            passwordHash = passwordHash.ToLower();

            return Collection.Use((o) =>
                o.Where(u => u.Username == username && u.PasswordHash == passwordHash).Any());
        }
    }
}
