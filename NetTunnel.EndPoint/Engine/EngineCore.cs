using NetTunnel.EndPoint.Engine.Managers;
using NetTunnel.Library.Types;

namespace NetTunnel.EndPoint.Engine
{
    public class EngineCore
    {
        public Logger Log { get; set; }
        public UserSessionManager Sessions { get; set; }
        public OutgoingEndpointManager OutgoingEndpoints { get; set; }
        public IncommingEndpointManager IncommingEndpoints { get; set; }
        public UserManager Users { get; set; }

        public EngineCore(NtEndpointServiceConfiguration config)
        {
            Log = new(this);
            Sessions = new(this);
            OutgoingEndpoints = new(this);
            IncommingEndpoints = new(this);
            Users = new(this);
        }

        public void Start()
        {
            IncommingEndpoints.StartAll();
            OutgoingEndpoints.StartAll();
        }

        public void Stop()
        {
            IncommingEndpoints.StopAll();
            OutgoingEndpoints.StopAll();
        }
    }
}
