using Topshelf;

namespace NetTunnel.EndPoint
{
    public class Program
    {
        public static void Main()
        {
            HostFactory.Run(x =>
            {
                x.StartAutomatically();

                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                });

                x.Service<NetTunnelService>(s =>
                {
                    s.ConstructUsing(hostSettings => new NetTunnelService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Omni-directional network tunneling endpoint.");
                x.SetDisplayName("NetTunnel.EndPoint");
                x.SetServiceName("NetTunnel.EndPoint");
            });
        }
    }
}
