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

        public void Add(User user) => _collection.Use((o) =>
        {
            o.Add(user);
            SaveToDisk();
        });

        public void Delete(string username) => _collection.Use((o) =>
        {
            o.RemoveAll(t => t.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            SaveToDisk();
        });

        public void EditUser(User user)
        {
            _collection.Use((o) =>
            {
                o.FirstOrDefault(t => t.Username.Equals(user.Username, StringComparison.CurrentCultureIgnoreCase))
                    ?.Modify(user);

                SaveToDisk();
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

        /// <summary>
        /// Saves the users to disk. The secrets (password hashes and certificate passwords) are written protected, see <see cref="SecretProtector"/>.
        /// </summary>
        public void SaveToDisk()
            => _collection.Use((o) =>
            {
                var users = o.Select(user => new User
                {
                    Username = user.Username,
                    PasswordHash = SecretProtector.Protect(user.PasswordHash),
                    Role = user.Role,
                    Endpoints = user.Endpoints.Select(endpoint =>
                    {
                        var clone = endpoint.CloneConfiguration();
                        clone.TlsCertificatePassword = SecretProtector.Protect(clone.TlsCertificatePassword) ?? string.Empty;
                        return clone;
                    }).ToList()
                }).ToList();

                CommonApplicationData.SaveToDisk(FriendlyName, users);
            });

        private void LoadFromDisk()
        {
            _collection.Use((o) =>
            {
                o.Clear();

                //Add directly rather than through Add() so that loading doesn't rewrite the file once per user.
                o.AddRange(CommonApplicationData.LoadFromDisk(FriendlyName, new List<User>()));

                //Secrets in files written by an older version (or typed in by hand) are plain text: read them, then save them protected.
                bool hasPlainTextSecrets = false;

                foreach (var user in o)
                {
                    user.PasswordHash = SecretProtector.Unprotect(user.PasswordHash, $"the password of user '{user.Username}'", out var plain);
                    hasPlainTextSecrets |= plain;

                    foreach (var endpoint in user.Endpoints)
                    {
                        endpoint.TlsCertificatePassword = SecretProtector.Unprotect(endpoint.TlsCertificatePassword,
                            $"the certificate password of endpoint '{endpoint.Name}' (user '{user.Username}')", out plain) ?? string.Empty;
                        hasPlainTextSecrets |= plain;
                    }
                }

                if (hasPlainTextSecrets)
                {
                    SaveToDisk();
                }

                if (o.Count == 0)
                {
#if DEBUG
                    o.Add(new User("debug", Utility.ComputeSha256Hash("123456789"), NtUserRole.Administrator));
#endif
                    o.Add(new User("root", Utility.ComputeSha256Hash(Environment.MachineName.ToLower()), NtUserRole.Administrator));
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
