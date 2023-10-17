using NetTunnel.Library.Config;

namespace NetTunnel.Engine
{
    public class EngineCore
    {
        public Logger Log { get; set; }
        public UserSessions Sessions { get; set; }

        public EngineCore(EndpointServiceConfiguration config)
        {
            Log = new(this);
            Sessions = new(this);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

    }
}
