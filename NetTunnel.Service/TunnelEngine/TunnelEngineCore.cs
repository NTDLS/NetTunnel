using NetTunnel.Service.TunnelEngine.Managers;

namespace NetTunnel.Service.TunnelEngine
{
    internal class TunnelEngineCore
    {
        public Logger Logging { get; set; }
        public UserSessionManager Sessions { get; set; }
        public TunnelOutboundManager OutboundTunnels { get; set; }
        public TunnelInboundManager InboundTunnels { get; set; }
        public UserManager Users { get; set; }

        public TunnelEngineCore()
        {
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
