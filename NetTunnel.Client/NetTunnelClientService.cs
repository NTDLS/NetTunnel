using System.ServiceProcess;

namespace NetTunnel.Client
{
    partial class NetTunnelClientService : ServiceBase
    {
        RoutingServices _routingServices = new RoutingServices();

        public NetTunnelClientService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _routingServices.Start();
        }

        protected override void OnStop()
        {
            _routingServices.Stop();
        }
    }
}
