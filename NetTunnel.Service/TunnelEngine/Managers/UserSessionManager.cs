using NetTunnel.Library.Types;
using NTDLS.Semaphore;

namespace NetTunnel.Service.TunnelEngine.Managers
{
    internal class UserSessionManager
    {
        private readonly TunnelEngineCore _core;

        private readonly PessimisticCriticalResource<List<NtUserSession>> _collection = new();

        public UserSessionManager(TunnelEngineCore core)
        {
            _core = core;
        }

        public NtUserSession? Login(string username, string passwordHash, string? clientIpAddress)
        {
            if (_core.Users.ValidateLogin(username, passwordHash))
            {
                var session = new NtUserSession(username, clientIpAddress);
                _collection.Use((o) => o.Add(session));
                return session;
            }
            return null;
        }

        public NtUserSession Validate(Guid sessionId, string? clientIpAddress)
        {
            var session = _collection.Use((o) => o.Where(o => o.SessionId == sessionId).FirstOrDefault())
                ?? throw new Exception("Session was not found.");

            if (session.ClientIpAddress != clientIpAddress)
            {
                throw new Exception("Session IP address mismatch.");
            }

            return session;
        }

        public void Logout(NtUserSession session) => _collection.Use((o) => { o.Remove(session); });
    }
}
