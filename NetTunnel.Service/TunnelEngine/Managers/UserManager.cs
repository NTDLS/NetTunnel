using NetTunnel.Library;
using NTDLS.Persistence;
using NTDLS.Semaphore;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class UserManager
    {
        private readonly TunnelEngineCore _core;

        private readonly PessimisticCriticalResource<List<NtUser>> _collection = new();

        public UserManager(TunnelEngineCore core)
        {
            _core = core;

            LoadFromDisk();
        }

        public void Add(string username, string passwordHash) => Add(new NtUser(username, passwordHash));
        public void Add(NtUser user) => _collection.Use((o) => o.Add(user));
        public void Delete(NtUser user) => _collection.Use((o) => o.RemoveAll(t => t.Username == user.Username));

        public void ChangePassword(NtUser user)
        {
            _collection.Use((o) =>
            {
                o.Where(x => x.Username == user.Username).FirstOrDefault()?
                    .SetPasswordHash(user.PasswordHash);
            });
        }

        public bool ValidateLogin(string username, string passwordHash)
        {
            username = username.ToLower();
            passwordHash = passwordHash.ToLower();

            return _collection.Use((o) =>
                o.Where(u => u.Username == username && u.PasswordHash == passwordHash).Any());
        }

        public List<NtUser> Clone()
        {
            return _collection.Use((o) => new List<NtUser>(o));
        }

        public void SaveToDisk() => CommonApplicationData.SaveToDisk(Constants.FriendlyName, Clone());

        private void LoadFromDisk()
        {
            _collection.Use((o) =>
            {
                if (o.Count != 0) throw new Exception("Can not load configuration on top of existing collection.");

                CommonApplicationData.LoadFromDisk<List<NtUser>>(Constants.FriendlyName)?.ForEach(o => Add(o));

                if (o.Count == 0)
                {
#if DEBUG
                    Add("debug", Utility.ComputeSha256Hash("123456789"));
#endif
                    Add("root", Utility.ComputeSha256Hash(Environment.MachineName.ToLower()));
                    SaveToDisk();
                }
            });
        }
    }
}
