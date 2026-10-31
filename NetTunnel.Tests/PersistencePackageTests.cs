using NetTunnel.Library.Payloads;
using NTDLS.Persistence;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// Goes through the NTDLS.Persistence package itself (2.1.0 stores enums as names and still reads numbers), in a temporary directory so
    /// that the machine's real configuration is never touched. This is the exact path the service uses for its configuration.
    /// </summary>
    public class PersistencePackageTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "nettunnel-tests-" + Guid.NewGuid());

        public void Dispose()
        {
            try { Directory.Delete(_directory, true); } catch { /* best effort */ }
        }

        [Fact]
        public void ServiceConfiguration_IsStoredWithEnumNames_AndLoadsBack()
        {
            var configuration = new ServiceConfiguration { LogLevel = NtLogSeverity.Warning, ServicePort = 4242 };

            PathPersistence.SaveToDisk(_directory, configuration);

            var text = File.ReadAllText(Path.Combine(_directory, "ServiceConfiguration.json"));
            Assert.Contains("\"LogLevel\": \"Warning\"", text);

            var loaded = PathPersistence.LoadFromDisk<ServiceConfiguration>(_directory);
            Assert.Equal(NtLogSeverity.Warning, loaded?.LogLevel);
            Assert.Equal(4242, loaded?.ServicePort);
        }

        [Fact]
        public void FilesWrittenByEarlierVersions_StillLoad()
        {
            Directory.CreateDirectory(_directory);

            //An older build wrote the names; the 2.0.1 package wrote numbers. Both must load.
            File.WriteAllText(Path.Combine(_directory, "ServiceConfiguration.json"), """{ "ServicePort": 1, "LogLevel": "Verbose" }""");
            Assert.Equal(NtLogSeverity.Verbose, PathPersistence.LoadFromDisk<ServiceConfiguration>(_directory)?.LogLevel);

            File.WriteAllText(Path.Combine(_directory, "ServiceConfiguration.json"), $$"""{ "ServicePort": 1, "LogLevel": {{(int)NtLogSeverity.Warning}} }""");
            Assert.Equal(NtLogSeverity.Warning, PathPersistence.LoadFromDisk<ServiceConfiguration>(_directory)?.LogLevel);
        }

        [Fact]
        public void EndpointsAndRules_RoundTripThroughThePackage()
        {
            var endpoints = new List<EndpointConfiguration>
            {
                new()
                {
                    EndpointId = Guid.NewGuid(),
                    Direction = NtDirection.Inbound,
                    InboundTrafficType = NtTrafficType.Https,
                    HttpHeaderRules = [new HttpHeaderRule(NtHttpVerb.Post, "X-Test", NtHttpHeaderAction.Upsert, "1") { HeaderType = NtHttpHeaderType.Request, Enabled = true }]
                }
            };

            PathPersistence.SaveToDisk(_directory, endpoints);
            var loaded = PathPersistence.LoadFromDisk<List<EndpointConfiguration>>(_directory);

            Assert.NotNull(loaded);
            Assert.Equal(NtDirection.Inbound, loaded[0].Direction);
            Assert.Equal(NtTrafficType.Https, loaded[0].InboundTrafficType);
            Assert.Equal(NtHttpVerb.Post, loaded[0].HttpHeaderRules[0].Verb);
        }
    }
}
