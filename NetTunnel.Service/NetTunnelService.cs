using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NetTunnel.Service
{
    partial class NetworkDlsNetTunnelService : ServiceBase
    {
        RoutingServices _routingServices = new RoutingServices();

        public NetworkDlsNetTunnelService()
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
