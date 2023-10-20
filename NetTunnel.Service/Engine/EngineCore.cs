using NetTunnel.Library.Types;
using NetTunnel.Service.Engine.Managers;

namespace NetTunnel.Service.Engine
{
    public class EngineCore
    {
        public Logger Logging { get; set; }
        public UserSessionManager Sessions { get; set; }
        public TunnelOutboundManager OutgoingTunnels { get; set; }
        public TunnelInboundManager IncomingTunnels { get; set; }
        public UserManager Users { get; set; }

        public EngineCore(NtServiceApplicationConfiguration config)
        {
            Logging = new(this);
            Sessions = new(this);
            OutgoingTunnels = new(this);
            IncomingTunnels = new(this);
            Users = new(this);
        }

        public void Start()
        {
            IncomingTunnels.StartAll();
            OutgoingTunnels.StartAll();
        }

        public void Stop()
        {
            IncomingTunnels.StopAll();
            OutgoingTunnels.StopAll();
        }
    }
}
