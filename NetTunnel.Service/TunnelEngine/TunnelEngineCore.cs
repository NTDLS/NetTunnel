using NetTunnel.Service.TunnelEngine.Managers;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine
{
    internal class TunnelEngineCore
    {
        public RmServer CoreServer { get; private set; }
        public Logger Logging { get; set; }
        public UserSessionManager Sessions { get; set; }
        public TunnelOutboundManager OutboundTunnels { get; set; }
        public TunnelInboundManager InboundTunnels { get; set; }
        public UserManager Users { get; set; }

        public TunnelEngineCore()
        {
            CoreServer = new RmServer();

            Logging = new(this);
            Sessions = new(this);
            OutboundTunnels = new(this);
            InboundTunnels = new(this);
            Users = new(this);
        }

        public void Start()
        {
            CoreServer.Start(Singletons.Configuration.ManagementPort);

            InboundTunnels.StartAll();
            OutboundTunnels.StartAll();
        }

        public void Stop()
        {
            InboundTunnels.StopAll();
            OutboundTunnels.StopAll();

            CoreServer.Stop();
        }
    }
}
