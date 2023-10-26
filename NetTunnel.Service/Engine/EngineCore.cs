using NetTunnel.Library.Types;
using NetTunnel.Service.Engine.Managers;

namespace NetTunnel.Service.Engine
{
    public class EngineCore
    {
        public Logger Logging { get; set; }
        public UserSessionManager Sessions { get; set; }
        public TunnelOutboundManager OutboundTunnels { get; set; }
        public TunnelInboundManager InboundTunnels { get; set; }
        public UserManager Users { get; set; }
        public NtServiceApplicationConfiguration Configuration { get; private set; }

        public EngineCore(NtServiceApplicationConfiguration configuration)
        {
            Configuration = configuration;
            Logging = new(this);
            Sessions = new(this);
            OutboundTunnels = new(this);
            InboundTunnels = new(this);
            Users = new(this);
        }

        public void Start()
        {
            InboundTunnels.StartAll();
            OutboundTunnels.StartAll();
        }

        public void Stop()
        {
            InboundTunnels.StopAll();
            OutboundTunnels.StopAll();
        }
    }
}
