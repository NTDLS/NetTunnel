using NetTunnel.Service.TunnelEngine;
using System.Diagnostics;
using Topshelf;

namespace NetTunnel.Service
{
    public class Program
    {
        public static void Main()
        {
            //NTDLS.Semaphore.ThreadOwnershipTracking.Enable();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                //Make sure the cause of a crash reaches the log (and event log) before the process goes away.
                if (e.ExceptionObject is Exception ex)
                {
                    Singletons.Logger.Exception(ex, "Unhandled exception");
                }
                if (e.IsTerminating)
                {
                    Singletons.ShutdownLogger();
                }
            };

            Singletons.Logger.Verbose("Starting...");

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

            Singletons.ShutdownLogger(); //Write out anything still queued.
        }
    }
}
