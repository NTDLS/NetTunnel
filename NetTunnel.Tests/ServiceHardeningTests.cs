using System.Globalization;
using System.Reflection;
using NetTunnel.Library;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.TunnelEngine;
using NetTunnel.Service.TunnelEngine.Managers;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// Tests which use the service's users, tunnels or configuration. They share the data folder of the test run, so they do not run
    /// at the same time as each other.
    /// </summary>
    [CollectionDefinition("ServiceState", DisableParallelization = true)]
    public class ServiceStateCollection;

    [Collection("ServiceState")]
    public class ServiceHardeningTests
    {
        private static UserManager NewUserManager() => new(null!);

        private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..20];

        [Fact]
        public void UserManager_RejectsAUserWhoseNameIsAlreadyTaken_WhateverTheCase()
        {
            var users = NewUserManager();
            var name = UniqueName("dup");
            users.Add(name, Utility.ComputeSha256Hash("one"), NtUserRole.Limited);

            var error = Assert.Throws<InvalidOperationException>(() => users.Add(name.ToUpperInvariant(), Utility.ComputeSha256Hash("two"), NtUserRole.Administrator));
            Assert.Contains("already exists", error.Message);

            //The original user can still log in; a second user of the same name would have made every lookup throw.
            Assert.Equal(NtUserRole.Limited, users.ValidateLoginAndGetRole(name, Utility.ComputeSha256Hash("one")));
            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(name, Utility.ComputeSha256Hash("two")));

            users.Delete(name);
        }

        [Fact]
        public void Login_RequiresTheRightPassword_AndIgnoresTheCaseOfNameAndHash()
        {
            var users = NewUserManager();
            var name = UniqueName("login");
            var hash = Utility.ComputeSha256Hash("correct horse");
            users.Add(name, hash, NtUserRole.Administrator);

            Assert.Equal(NtUserRole.Administrator, users.ValidateLoginAndGetRole(name, hash));
            Assert.Equal(NtUserRole.Administrator, users.ValidateLoginAndGetRole(name.ToUpperInvariant(), hash.ToUpperInvariant()));

            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(name, Utility.ComputeSha256Hash("wrong")));
            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(name, hash[..^1])); //A prefix of the hash.
            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(name, hash + "0"));
            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(name, string.Empty));
            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(string.Empty, hash));
            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole("nobody-" + name, hash));

            users.Delete(name);
        }

        [Fact]
        public void Login_IsNotAffectedByTheMachinesCulture()
        {
            //In Turkish, "I" and "i" are not each other's upper and lower case, which broke the name comparison for names with an "i".
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

                var users = NewUserManager();
                var name = "Isaac-" + Guid.NewGuid().ToString("N")[..8];
                var hash = Utility.ComputeSha256Hash("pw");
                users.Add(name, hash, NtUserRole.Limited);

                Assert.Equal(NtUserRole.Limited, users.ValidateLoginAndGetRole(name.ToUpperInvariant(), hash));
                Assert.Equal(NtUserRole.Limited, users.ValidateLoginAndGetRole(name.ToLowerInvariant(), hash));
                Assert.Equal(name.ToLowerInvariant(), new User(name, hash, NtUserRole.Limited).Username);

                users.Delete(name);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void User_WithoutAPasswordHash_CanNeverLogIn()
        {
            var users = NewUserManager();
            var name = UniqueName("nohash");
            users.Add(new User(name, null, NtUserRole.Administrator));

            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(name, string.Empty));
            Assert.Equal(NtUserRole.Undefined, users.ValidateLoginAndGetRole(name, "anything"));

            users.Delete(name);
        }

        [Fact]
        public void Configuration_AcceptsTheDefaults()
        {
            Singletons.ValidateConfiguration(new ServiceConfiguration());
        }

        [Theory]
        [InlineData(nameof(ServiceConfiguration.ServicePort), 0)]
        [InlineData(nameof(ServiceConfiguration.ServicePort), 70000)]
        [InlineData(nameof(ServiceConfiguration.InitialReceiveBufferSize), 0)]
        [InlineData(nameof(ServiceConfiguration.InitialReceiveBufferSize), 100)]
        [InlineData(nameof(ServiceConfiguration.MaxReceiveBufferSize), 1024)] //Smaller than the initial size.
        [InlineData(nameof(ServiceConfiguration.PingCadence), -1)]
        [InlineData(nameof(ServiceConfiguration.MessageQueryTimeoutMs), 0)]
        [InlineData(nameof(ServiceConfiguration.EndpointHeartbeatDelayMs), 0)]
        [InlineData(nameof(ServiceConfiguration.StaleEndpointExpirationMs), -5)]
        [InlineData(nameof(ServiceConfiguration.TunnelCryptographyKeySize), 100)]
        [InlineData(nameof(ServiceConfiguration.LogFileMaxSizeMB), 0)]
        [InlineData(nameof(ServiceConfiguration.LogFileRetainedCount), -1)]
        public void Configuration_RefusesValuesThatWouldStopTheServiceFromWorking(string property, int value)
        {
            var configuration = new ServiceConfiguration();
            typeof(ServiceConfiguration).GetProperty(property)!.SetValue(configuration, value);

            Assert.Throws<ArgumentException>(() => Singletons.ValidateConfiguration(configuration));
        }

        [Fact]
        public void Configuration_RefusesAGrowthRateOfZero()
        {
            Assert.Throws<ArgumentException>(() => Singletons.ValidateConfiguration(new ServiceConfiguration { ReceiveBufferGrowthRate = 0 }));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 4)]
        [InlineData(4, 8)]
        [InlineData(5, 16)]
        [InlineData(6, 30)]
        [InlineData(50, 30)]
        public void ReconnectDelay_GrowsToHalfAMinute(int failedAttempts, int expectedSeconds)
        {
            Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), TunnelOutbound.RetryDelay(failedAttempts));
        }

        [Fact]
        public void OutboundTunnel_StartedTwice_RunsOneConnectionThread()
        {
            var configuration = new TunnelConfiguration
            {
                TunnelId = Guid.NewGuid(),
                ServiceId = Singletons.Configuration.ServiceId,
                Name = "double-start",
                Address = "127.0.0.1",
                ServicePort = LoopbackTunnel.FreePort(), //Nothing listens here; the tunnel just keeps trying.
                Username = "nobody",
                PasswordHash = Utility.ComputeSha256Hash("x")
            };

            var tunnel = new TunnelOutbound(Singletons.ServiceEngine, configuration);
            var threadField = typeof(TunnelOutbound).GetField("_establishConnectionThread", BindingFlags.NonPublic | BindingFlags.Instance)!;

            try
            {
                tunnel.Start();
                var first = threadField.GetValue(tunnel);
                Assert.NotNull(first);

                tunnel.Start();
                Assert.Same(first, threadField.GetValue(tunnel));
            }
            finally
            {
                tunnel.Stop();
            }
        }
    }
}
