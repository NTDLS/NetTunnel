using NTDLS.SecureKeyExchange;
using Topshelf;

namespace NetTunnel.Service
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

                x.SetDescription("Omni-directional network tunneling service.");
                x.SetDisplayName("NetworkDLS NetTunnel");
                x.SetServiceName("NtNetTunnel");
            });
        }
    }
}
