using NetTunnel.Library;
using NTDLS.Persistence;
using NTDLS.Semaphore;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class UserManager
    {
        private readonly ServiceEngine _serviceEngine;

        private readonly PessimisticCriticalResource<List<User>> _collection = new();

        public UserManager(ServiceEngine serviceEngine)
        {
            _serviceEngine = serviceEngine;

            LoadFromDisk();
        }

        public void Add(string username, string passwordHash)
            => Add(new User(username, passwordHash));

        public void Add(User user) => _collection.Use((o)
            => o.Add(user));

        public void Delete(User user) => _collection.Use((o)
            => o.RemoveAll(t => t.Username == user.Username));

        public void ChangePassword(User user)
        {
            _collection.Use((o) =>
            {
                o.Where(x => x.Username == user.Username).FirstOrDefault()?
                    .SetPasswordHash(user.PasswordHash);
            });
        }

        public bool ValidateLogin(string username, string passwordHash)
        {
            return _collection.Use((o) =>
                o.Where(u => u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)
                && u.PasswordHash.Equals(passwordHash, StringComparison.CurrentCultureIgnoreCase)).Any());
        }

        public bool ValidatePassword(string username, string passwordHash)
        {
            if (_serviceEngine.Users.ValidateLogin(username, passwordHash))
            {
                return true;
            }
            return false;
        }

        public void SaveToDisk()
            => _collection.Use((o) => CommonApplicationData.SaveToDisk(Constants.FriendlyName, o));

        private void LoadFromDisk()
        {
            _collection.Use((o) =>
            {
                o.Clear();

                CommonApplicationData.LoadFromDisk(Constants.FriendlyName, new List<User>()).ForEach(o => Add(o));

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
