using NTDLS.Semaphore;

namespace NetTunnel.EndPoint.Engine.Managers
{
    public class UserManager
    {
        private readonly EngineCore _core;

        private CriticalResource<List<NtUser>> _collection = new();

        public UserManager(EngineCore core)
        {
            _core = core;
        }

        public void Add(string username, string passwordHash) => Add(new NtUser(username, passwordHash));

        public void Add(NtUser user) => _collection.Use((o) => o.Add(user));

        public bool ValidateLogin(string username, string passwordHash)
        {
            username = username.ToLower();
            passwordHash = passwordHash.ToLower();

            return _collection.Use((o) =>
                o.Where(u => u.Username == username && u.PasswordHash == passwordHash).Any());
        }

        public List<NtUser> Clone()
        {
            return _collection.Use((o) =>
            {
                List<NtUser> clones = new();
                foreach (var user in o)
                {
                    clones.Add(user.Clone());
                }
                return clones;
            });
        }
    }
}
