using System.Diagnostics;
using Topshelf;

namespace NetTunnel.Service
{
    public class Program
    {
        public static void Main()
        {
            //NTDLS.Semaphore.ThreadOwnershipTracking.Enable();

            HostFactory.Run(x =>
            {
                x.StartAutomatically();

                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                });

                x.Service<SystemService>(s =>
                {
                    s.ConstructUsing(hostSettings => new SystemService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("Omni-directional network tunneling service.");
                x.SetDisplayName("NetworkDLS NetTunnel");
                x.SetServiceName("NtNetTunnel");

                x.BeforeInstall(() =>
                {
                    if (!EventLog.SourceExists(Library.Constants.EventSourceName))
                    {
                        EventLog.CreateEventSource(Library.Constants.EventSourceName, "Application");
                    }
                });
            });
        }
    }
}
