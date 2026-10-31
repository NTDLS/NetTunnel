using System.Runtime.CompilerServices;
using NetTunnel.Library.Payloads;
using NTDLS.Persistence;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// Gives the whole test run a service data folder of its own, before anything in the service reads its configuration, so that the
    /// tests never read or change the configuration, users, tunnels or secret key of an installed NetTunnel. The folder is deleted when
    /// the run ends.
    /// </summary>
    internal static class TestSandbox
    {
        public static string DataFolderName { get; } = $"NetTunnel.Tests-{Guid.NewGuid():N}";

        [ModuleInitializer]
        internal static void Initialize()
        {
            Environment.SetEnvironmentVariable(DataFolderNameVariable, DataFolderName);

            CommonApplicationData.SaveToDisk(DataFolderName, new ServiceConfiguration
            {
                LogLevel = NtLogSeverity.Warning,
                LogToEventLog = false,
                PingCadence = 0
            });

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                try
                {
                    Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), DataFolderName), true);
                }
                catch
                {
                    //Best effort.
                }
            };
        }
    }
}
