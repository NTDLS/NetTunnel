using System.Text.Json;
using NetTunnel.Library.Payloads;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// NTDLS.Persistence reads and writes the configuration files with System.Text.Json. Enums must be stored as names (which is what the
    /// documentation shows and what earlier versions wrote), yet files holding numbers must still load. These tests use the serializer
    /// directly so that they never touch the machine's real configuration.
    /// </summary>
    public class PersistenceFormatTests
    {
        [Fact]
        public void ServiceConfiguration_WithEnumNames_Loads()
        {
            var configuration = JsonSerializer.Deserialize<ServiceConfiguration>("""{ "ServicePort": 52845, "LogLevel": "Verbose" }""");

            Assert.NotNull(configuration);
            Assert.Equal(NtLogSeverity.Verbose, configuration.LogLevel);
        }

        [Fact]
        public void ServiceConfiguration_WithEnumNumbers_StillLoads()
        {
            var configuration = JsonSerializer.Deserialize<ServiceConfiguration>($$"""{ "LogLevel": {{(int)NtLogSeverity.Warning}} }""");

            Assert.NotNull(configuration);
            Assert.Equal(NtLogSeverity.Warning, configuration.LogLevel);
        }

        [Fact]
        public void EndpointConfiguration_RoundTripsWithNames()
        {
            var endpoint = new EndpointConfiguration
            {
                EndpointId = Guid.NewGuid(),
                Direction = NtDirection.Outbound,
                InboundTrafficType = NtTrafficType.Https,
                OutboundTrafficType = NtTrafficType.Http
            };
            endpoint.HttpHeaderRules.Add(new HttpHeaderRule(NtHttpVerb.Post, "X-Test", NtHttpHeaderAction.Upsert, "1")
            { HeaderType = NtHttpHeaderType.Request, Enabled = true });

            var json = JsonSerializer.Serialize(endpoint);

            Assert.Contains("\"Outbound\"", json);
            Assert.Contains("\"Https\"", json);
            Assert.Contains("\"Upsert\"", json);
            Assert.Contains("\"Post\"", json);

            var loaded = JsonSerializer.Deserialize<EndpointConfiguration>(json);
            Assert.NotNull(loaded);
            Assert.Equal(endpoint.Direction, loaded.Direction);
            Assert.Equal(endpoint.InboundTrafficType, loaded.InboundTrafficType);
            Assert.Equal(NtHttpVerb.Post, loaded.HttpHeaderRules.Single().Verb);
            Assert.Equal(NtHttpHeaderType.Request, loaded.HttpHeaderRules.Single().HeaderType);
        }
    }
}
