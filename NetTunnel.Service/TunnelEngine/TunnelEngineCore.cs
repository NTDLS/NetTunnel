using NetTunnel.Service.ReliableMessages.Handlers;
using NetTunnel.Service.TunnelEngine.Managers;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    internal class TunnelEngineCore
    {
        public Dictionary<Guid, ServiceConnectionContext> InboundTunnelConnections { get; private set; } = new();
        public RmServer CoreServer { get; private set; }
        public Logger Logging { get; set; }
        public UserSessionManager Sessions { get; set; }
        public TunnelManager OutboundTunnels { get; set; }
        public UserManager Users { get; set; }

        public TunnelEngineCore()
        {
            Logging = new(this);
            Sessions = new(this);
            OutboundTunnels = new(this);
            Users = new(this);

            CoreServer = new RmServer();

            CoreServer = new RmServer(new RmConfiguration()
            {
                Parameter = this,
                //FrameDelimiter = Singletons.Configuration.FrameDelimiter,
                InitialReceiveBufferSize = Singletons.Configuration.InitialReceiveBufferSize,
                MaxReceiveBufferSize = Singletons.Configuration.MaxReceiveBufferSize,
                ReceiveBufferGrowthRate = Singletons.Configuration.ReceiveBufferGrowthRate,
            });

            CoreServer.AddHandler(new ServiceNotificationHandlers());
            CoreServer.AddHandler(new ServiceQueryHandlers());

            CoreServer.OnConnected += CoreServer_OnConnected;
            CoreServer.OnDisconnected += CoreServer_OnDisconnected;

            CoreServer.OnException += (RmContext? context, Exception ex, IRmPayload? payload) =>
            {
                Logging.Write(NtLogSeverity.Exception, $"RPC server exception: '{ex.Message}'"
                    + (payload != null ? $", Payload: {payload?.GetType()?.Name}" : string.Empty));
            };
        }

        private void CoreServer_OnConnected(RmContext context)
        {
            InboundTunnelConnections.Add(context.ConnectionId,
                new ServiceConnectionContext(context.ConnectionId));
        }

        private void CoreServer_OnDisconnected(RmContext context)
        {
            Sessions.Logout(context.ConnectionId);
            InboundTunnelConnections.Remove(context.ConnectionId);
        }

        public void Start()
        {
            CoreServer.SetCryptographyProvider(new ServiceCryptographyProvider(this));

            CoreServer.Start(Singletons.Configuration.ManagementPort);

            OutboundTunnels.StartAll();
        }

        public void Stop()
        {
            OutboundTunnels.StopAll();

            CoreServer.Stop();
        }
    }
}
