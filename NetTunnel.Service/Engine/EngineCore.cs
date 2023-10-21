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

        public EngineCore(NtServiceApplicationConfiguration config)
        {
            Logging = new(this);
            Sessions = new(this);
            OutboundTunnels = new(this);
            InboundTunnels = new(this);
            Users = new(this);
        }

        /*
        public void GetAllTunnels()
        {
            var outbound = OutboundTunnels.GetBasicInfo();
            var inbound = InboundTunnels.GetBasicInfo();
        }
        */

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
