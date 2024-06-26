using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NTDLS.Persistence;
using NTDLS.Semaphore;
using static NetTunnel.Library.Constants;

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

        public void Add(string username, string passwordHash, NtUserRole role)
            => Add(new User(username, passwordHash, role));

        public void Add(User user) => _collection.Use((o)
            => o.Add(user));

        public void Delete(string username) => _collection.Use((o)
            => o.RemoveAll(t => t.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)));

        public void EditUser(User user)
        {
            _collection.Use((o) =>
            {
                o.FirstOrDefault(t => t.Username.Equals(user.Username, StringComparison.CurrentCultureIgnoreCase))
                    ?.Modify(user);
            });
        }

        public NtUserRole ValidateLoginAndGetRole(string username, string passwordHash)
        {
            return _collection.Use((o) =>
                o.SingleOrDefault(u =>
                    u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)
                    && u.PasswordHash != null
                    && u.PasswordHash.Equals(passwordHash, StringComparison.CurrentCultureIgnoreCase))
                )?.Role ?? NtUserRole.Undefined;
        }

        public List<User> Clone()
        {
            var results = new List<User>();
            _collection.Use((o) => o.ForEach(u => results.Add(u)));
            return results;
        }

        public List<EndpointConfiguration> GetEndpoints(string username)
        {
            var clones = new List<EndpointConfiguration>();

            _collection.Use((o) =>
            {
                var user = o.SingleOrDefault(u => u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
                if (user != null)
                {
                    foreach (var endpoint in user.Endpoints)
                    {
                        clones.Add(endpoint.CloneConfiguration());
                    }
                }
            });

            return clones;
        }

        /// <summary>
        /// Adds an endpoint to a user account
        /// </summary>
        public void UpsertEndpoint(string username, EndpointConfiguration endpoint)
        {
            _collection.Use((o) =>
            {
                var user = o.SingleOrDefault(u => u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));

                user?.Endpoints.RemoveAll(o => o.EndpointId == endpoint.EndpointId);
                user?.Endpoints.Add(endpoint);

                SaveToDisk();
            });
        }

        /// <summary>
        /// Adds an endpoint to a user account
        /// </summary>
        public void DeleteEndpoint(string username, Guid endpointId)
        {
            _collection.Use((o) =>
            {
                var user = o.SingleOrDefault(u => u.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
                user?.Endpoints.RemoveAll(o => o.EndpointId == endpointId);
                SaveToDisk();
            });
        }

        public void SaveToDisk()
            => _collection.Use((o) => CommonApplicationData.SaveToDisk(FriendlyName, o));

        private void LoadFromDisk()
        {
            _collection.Use((o) =>
            {
                o.Clear();

                CommonApplicationData.LoadFromDisk(FriendlyName, new List<User>()).ForEach(o => Add(o));

                if (o.Count == 0)
                {
#if DEBUG
                    Add("debug", Utility.ComputeSha256Hash("123456789"), NtUserRole.Administrator);
#endif
                    Add("root", Utility.ComputeSha256Hash(Environment.MachineName.ToLower()), NtUserRole.Administrator);
                    SaveToDisk();
                }

                foreach (var user in o)
                {
                    //Endpoints get a new ID every time they are loaded. This makes it easy to copy configs to other machines.
                    user.Endpoints.ForEach(e => e.EndpointId = Guid.NewGuid());
                }
            });
        }
    }
}
