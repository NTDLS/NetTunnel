using NetTunnel.EndPoint.Engine.Managers;
using NetTunnel.Library.Types;

namespace NetTunnel.EndPoint.Engine
{
    public class EngineCore
    {
        public Logger Logging { get; set; }
        public UserSessionManager Sessions { get; set; }
        public OutgoingEndpointManager OutgoingEndpoints { get; set; }
        public IncomingEndpointManager IncomingEndpoints { get; set; }
        public UserManager Users { get; set; }

        public EngineCore(NtEndpointServiceConfiguration config)
        {
            Logging = new(this);
            Sessions = new(this);
            OutgoingEndpoints = new(this);
            IncomingEndpoints = new(this);
            Users = new(this);
        }

        public void Start()
        {
            IncomingEndpoints.StartAll();
            OutgoingEndpoints.StartAll();
        }

        public void Stop()
        {
            IncomingEndpoints.StopAll();
            OutgoingEndpoints.StopAll();
        }
    }
}
