using NetTunnel.Library;
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

            LoadFromDisk();
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
                    clones.Add(user);
                }
                return clones;
            });
        }

        public void SaveToDisk() => Persistence.SaveToDisk(Clone());

        private void LoadFromDisk()
        {
            _collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");

                Persistence.LoadFromDisk<List<NtUser>>()?.ForEach(o => Add(o));
            });
        }
    }
}
