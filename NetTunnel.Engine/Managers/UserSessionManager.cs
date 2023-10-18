namespace NetTunnel.Engine.Managers
{
    public class UserSessionManager
    {
        private readonly EngineCore _core;

        public List<UserSession> Collection { get; set; } = new();

        public UserSessionManager(EngineCore core)
        {
            _core = core;
        }

        public UserSession? Login(string username, string passwordHash, string? clientIpAddress)
        {
            var session = new UserSession(username, clientIpAddress);

            if (username.ToLower() == "admin" && passwordHash == "abcdefg")
            {
                Collection.Add(session);

                return session;
            }
            return null;
        }

        public UserSession Acquire(Guid sessionId, string? clientIpAddress)
        {
            var session = Collection.Where(o => o.SessionId == sessionId).FirstOrDefault();
            if (session == null)
            {
                throw new Exception("Session was not found.");
            }

            if (session.ClientIpAddress != clientIpAddress)
            {
                throw new Exception("Session IP address mismatch.");
            }

            return session;
        }
    }
}
