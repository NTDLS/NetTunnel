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

        public bool Login(Guid connectionId, string username, string passwordHash, string? clientIpAddress)
        {
            if (_core.Users.ValidateLogin(username, passwordHash))
            {
                _collection.Use((o) => o.Add(new NtUserSession(connectionId, username, clientIpAddress)));
                return true;
            }
            return false;
        }

        public NtUserSession Validate(Guid sessionId, string? clientIpAddress)
        {
            var session = _collection.Use((o) => o.Where(o => o.ConnectionId == sessionId).FirstOrDefault())
                ?? throw new Exception("Session was not found.");

            if (session.ClientIpAddress != clientIpAddress)
            {
                throw new Exception("Session IP address mismatch.");
            }

            return session;
        }

        public void Logout(Guid connectionId)
        {
            _collection.Use((o) =>
            {
                o.RemoveAll(o => o.ConnectionId == connectionId);
            });
        }

    }
}
