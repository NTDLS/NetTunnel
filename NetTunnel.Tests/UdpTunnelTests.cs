using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using NetTunnel.Library.Payloads;
using NetTunnel.Service.TunnelEngine;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// UDP through a real inbound and a real outbound UDP endpoint over real sockets, joined by a tunnel which delivers its notifications
    /// concurrently and out of order the way the messaging layer does.
    /// </summary>
    [Collection("ServiceState")]
    public class UdpTunnelTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        private static void Udp(EndpointConfiguration configuration)
        {
            configuration.InboundTrafficType = NtTrafficType.Udp;
            configuration.OutboundTrafficType = NtTrafficType.Udp;
        }

        /// <summary>
        /// A UDP server which answers every datagram with the same bytes and remembers who it heard from.
        /// </summary>
        private sealed class UdpEchoServer : IDisposable
        {
            private readonly UdpClient _socket;
            private readonly CancellationTokenSource _stop = new();

            public int Port { get; }
            public ConcurrentDictionary<int, IPEndPoint> Clients { get; } = new();
            public IPEndPoint? LastClient { get; private set; }

            public UdpEchoServer(int port = 0)
            {
                _socket = new UdpClient(new IPEndPoint(IPAddress.Loopback, port));
                _socket.Client.ReceiveBufferSize = 4 * 1024 * 1024;
                Port = ((IPEndPoint)_socket.Client.LocalEndPoint!).Port;

                _ = Task.Run(async () =>
                {
                    while (!_stop.IsCancellationRequested)
                    {
                        try
                        {
                            var received = await _socket.ReceiveAsync(_stop.Token);
                            Clients[received.RemoteEndPoint.Port] = received.RemoteEndPoint;
                            LastClient = received.RemoteEndPoint;
                            await _socket.SendAsync(received.Buffer, received.RemoteEndPoint, _stop.Token);
                        }
                        catch (OperationCanceledException) { return; }
                        catch (ObjectDisposedException) { return; }
                        catch (SocketException) { /* a client that went away */ }
                    }
                });
            }

            public Task SendAsync(byte[] data, IPEndPoint to) => _socket.SendAsync(data, to).AsTask();

            public void Dispose()
            {
                _stop.Cancel();
                _socket.Dispose();
            }
        }

        private static UdpClient NewClient(LoopbackTunnel tunnel)
        {
            var client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            client.Client.ReceiveBufferSize = 4 * 1024 * 1024;
            client.Connect(IPAddress.Loopback, tunnel.ListenPort);
            return client;
        }

        private static async Task<byte[]> ReceiveAsync(UdpClient client)
        {
            using var cts = new CancellationTokenSource(Timeout);

            try
            {
                return (await client.ReceiveAsync(cts.Token)).Buffer;
            }
            catch (OperationCanceledException)
            {
                //Say what each end of the tunnel saw, so that a datagram lost now and then can be traced.
                throw new TimeoutException($"No datagram arrived within {Timeout.TotalSeconds:n0} seconds. {LoopbackTunnel.Last?.Describe()}");
            }
        }

        private static byte[] RandomBytes(int length, int seed)
        {
            var bytes = new byte[length];
            new Random(seed).NextBytes(bytes);
            return bytes;
        }

        private static async Task WaitUntilAsync(Func<bool> condition, string because)
        {
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(15);
            while (!condition() && DateTime.UtcNow < deadline)
            {
                await Task.Delay(25);
            }

            Assert.True(condition(), because);
        }

        [Fact]
        public async Task EveryDatagramIsDelivered_WithItsBoundariesAndBytes_AtAnySize()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);
            using var client = NewClient(tunnel);

            foreach (var size in new[] { 1, 2, 100, 512, 1400, 1472, 4000, 8192, 30000, 60000, UdpMaxDatagramSize })
            {
                var datagram = RandomBytes(size, size);
                await client.SendAsync(datagram);

                var echoed = await ReceiveAsync(client);
                Assert.True(datagram.AsSpan().SequenceEqual(echoed), $"a datagram of {size} bytes came back as {echoed.Length} bytes, or with different contents");
            }
        }

        [Fact]
        public async Task DatagramsOfOneClient_ArriveInTheOrderTheyWereSent()
        {
            //The tunnel hands the datagrams over concurrently and out of order; the session must put them back in sequence.
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);
            using var client = NewClient(tunnel);

            const int count = 400;
            for (int i = 0; i < count; i++)
            {
                var datagram = new byte[64];
                BitConverter.GetBytes(i).CopyTo(datagram, 0);
                await client.SendAsync(datagram);
            }

            for (int i = 0; i < count; i++)
            {
                var echoed = await ReceiveAsync(client);
                Assert.Equal(i, BitConverter.ToInt32(echoed, 0));
            }
        }

        [Fact]
        public async Task EachClientGetsASessionOfItsOwn_AndOnlyItsOwnReplies()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);

            var clients = Enumerable.Range(0, 8).Select(_ => NewClient(tunnel)).ToList();
            try
            {
                for (int round = 0; round < 5; round++)
                {
                    for (int c = 0; c < clients.Count; c++)
                    {
                        await clients[c].SendAsync(BitConverter.GetBytes(c * 1000 + round));
                    }

                    for (int c = 0; c < clients.Count; c++)
                    {
                        Assert.Equal(c * 1000 + round, BitConverter.ToInt32(await ReceiveAsync(clients[c])));
                    }
                }

                //The destination saw one source per client, not one for all of them: that is what lets it tell the clients apart.
                Assert.Equal(clients.Count, server.Clients.Count);
                Assert.Equal((ulong)clients.Count, tunnel.Inbound.CurrentConnections);
                Assert.Equal((ulong)clients.Count, tunnel.Outbound.CurrentConnections);
            }
            finally
            {
                clients.ForEach(c => c.Dispose());
            }
        }

        [Fact]
        public async Task TheDestinationCanSendMoreThanItWasAsked()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);
            using var client = NewClient(tunnel);

            await client.SendAsync("hello"u8.ToArray());
            Assert.Equal("hello", System.Text.Encoding.ASCII.GetString(await ReceiveAsync(client)));

            //A server may talk without being asked (a game server, a stream). It writes to the address it saw, and the client gets it.
            for (int i = 0; i < 5; i++)
            {
                await server.SendAsync(BitConverter.GetBytes(i), server.LastClient!);
            }

            for (int i = 0; i < 5; i++)
            {
                Assert.Equal(i, BitConverter.ToInt32(await ReceiveAsync(client)));
            }
        }

        [Fact]
        public async Task SessionsEndWhenTheirClientsGoQuiet_AtBothEndsOfTheTunnel()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);
            using var client = NewClient(tunnel);

            await client.SendAsync("one"u8.ToArray());
            await ReceiveAsync(client);

            Assert.Equal(1UL, tunnel.Inbound.CurrentConnections);
            Assert.Equal(1UL, tunnel.Outbound.CurrentConnections);

            //Nothing more is sent. UDP has no connection that closes, so only the idle timeout can end the session.
            using (WithSessionTimeout(1500))
            {
                await WaitUntilAsync(() => tunnel.Inbound.CurrentConnections == 0, "the inbound session never ended");
                await WaitUntilAsync(() => tunnel.Outbound.CurrentConnections == 0, "the outbound session (and its socket) never ended");
            }

            //The client is not shut out: its next datagram starts a new session.
            await client.SendAsync("two"u8.ToArray());
            Assert.Equal("two", System.Text.Encoding.ASCII.GetString(await ReceiveAsync(client)));
            Assert.Equal(2UL, tunnel.Inbound.TotalConnections);
        }

        /// <summary>
        /// Lowers the idle timeout for a test of idle sessions. The other tests keep a long one, so that a stall while the process warms
        /// up can never end a session with a datagram in flight.
        /// </summary>
        private static IDisposable WithSessionTimeout(int milliseconds)
        {
            var original = Singletons.Configuration.UdpSessionTimeoutMs;
            Singletons.Configuration.UdpSessionTimeoutMs = milliseconds;
            return new Restore(() => Singletons.Configuration.UdpSessionTimeoutMs = original);
        }

        private sealed class Restore(Action restore) : IDisposable
        {
            public void Dispose() => restore();
        }

        [Fact]
        public async Task ASessionThatKeepsTalking_IsNotEnded()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);
            using var client = NewClient(tunnel);

            //More than twice the idle timeout, with a datagram well inside it each time.
            using var timeout = WithSessionTimeout(3000);
            var until = DateTime.UtcNow + TimeSpan.FromSeconds(7);
            while (DateTime.UtcNow < until)
            {
                await client.SendAsync("tick"u8.ToArray());
                await ReceiveAsync(client);
                await Task.Delay(400);
            }

            Assert.Equal(1UL, tunnel.Inbound.TotalConnections);
            Assert.Equal(1UL, tunnel.Inbound.CurrentConnections);
        }

        [Fact]
        public async Task ClientsBeyondTheSessionLimit_AreDropped_AndOthersKeepWorking()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);

            var original = Singletons.Configuration.UdpMaxSessionsPerEndpoint;
            Singletons.Configuration.UdpMaxSessionsPerEndpoint = 2;
            try
            {
                using var first = NewClient(tunnel);
                using var second = NewClient(tunnel);
                using var third = NewClient(tunnel);

                await first.SendAsync(new byte[] { 1 });
                await second.SendAsync(new byte[] { 2 });
                Assert.Equal(1, (await ReceiveAsync(first))[0]);
                Assert.Equal(2, (await ReceiveAsync(second))[0]);

                await third.SendAsync(new byte[] { 3 });
                await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(700));
                    await third.ReceiveAsync(cts.Token);
                });

                //The sessions that exist are not harmed.
                await first.SendAsync(new byte[] { 11 });
                Assert.Equal(11, (await ReceiveAsync(first))[0]);
                Assert.Equal(2UL, tunnel.Inbound.CurrentConnections);
            }
            finally
            {
                Singletons.Configuration.UdpMaxSessionsPerEndpoint = original;
            }
        }

        [Fact]
        public async Task ADestinationThatIsNotThereYet_DoesNotEndTheSession()
        {
            //Nothing listens at first, so the network answers the first datagram with "port unreachable" (an error on the next receive on some
            //  systems). That must not end the session: the destination may be starting up.
            int port = LoopbackTunnel.FreePort();
            using var tunnel = new LoopbackTunnel(port, Udp);
            using var client = NewClient(tunnel);

            await client.SendAsync("anyone there?"u8.ToArray());
            await Task.Delay(400);

            using var server = new UdpEchoServer(port);

            await client.SendAsync("now?"u8.ToArray());
            Assert.Equal("now?", System.Text.Encoding.ASCII.GetString(await ReceiveAsync(client)));
            Assert.Equal(1UL, tunnel.Inbound.TotalConnections);
        }

        [Fact]
        public async Task EndpointWhosePortIsBusy_StartsReceivingOnceItIsFree()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp, start: false);

            using (new UdpClient(new IPEndPoint(IPAddress.Any, tunnel.ListenPort))) //Another program holds the port.
            {
                tunnel.Inbound.Start();
                await Task.Delay(500);
            }

            using var client = NewClient(tunnel);
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(20);
            byte[]? echoed = null;

            while (echoed == null && DateTime.UtcNow < deadline)
            {
                await client.SendAsync("anyone?"u8.ToArray());
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
                    echoed = (await client.ReceiveAsync(cts.Token)).Buffer;
                }
                catch (Exception ex) when (ex is OperationCanceledException or SocketException)
                {
                    //Not listening yet.
                }
            }

            Assert.NotNull(echoed);
            Assert.Equal("anyone?", System.Text.Encoding.ASCII.GetString(echoed));
        }

        [Fact]
        public async Task StoppingTheEndpoint_EndsItsSessionsAndReleasesThePort()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp);
            using var client = NewClient(tunnel);

            await client.SendAsync("x"u8.ToArray());
            await ReceiveAsync(client);

            tunnel.Inbound.Stop();

            Assert.Equal(0UL, tunnel.Inbound.CurrentConnections);
            await WaitUntilAsync(() => tunnel.Outbound.CurrentConnections == 0, "the outbound session was not told");

            //The port can be bound again by someone else.
            using var rebound = new UdpClient(new IPEndPoint(IPAddress.Any, tunnel.ListenPort));
        }

        [Fact]
        public void TheFactoryCreatesUdpEndpoints_ForUdpConfigurations()
        {
            using var server = new UdpEchoServer();
            using var tunnel = new LoopbackTunnel(server.Port, Udp, start: false);

            Assert.IsType<NetTunnel.Service.TunnelEngine.Endpoints.UdpEndpointInbound>(tunnel.Inbound);
            Assert.IsType<NetTunnel.Service.TunnelEngine.Endpoints.UdpEndpointOutbound>(tunnel.Outbound);
        }

        [Fact]
        public void Configuration_RefusesUdpThatContradictsItself()
        {
            EndpointConfiguration Valid() => new()
            {
                Name = "udp",
                InboundPort = 5000,
                OutboundPort = 6000,
                InboundTrafficType = NtTrafficType.Udp,
                OutboundTrafficType = NtTrafficType.Udp
            };

            Valid().Validate();

            Assert.Throws<ArgumentException>(() => { var c = Valid(); c.OutboundTrafficType = NtTrafficType.Raw; c.Validate(); });
            Assert.Throws<ArgumentException>(() => { var c = Valid(); c.InboundTrafficType = NtTrafficType.Https; c.Validate(); });
            Assert.Throws<ArgumentException>(() =>
            {
                var c = Valid();
                c.HttpHeaderRules.Add(new HttpHeaderRule(NtHttpVerb.Any, "X", NtHttpHeaderAction.Insert, "1"));
                c.Validate();
            });
            Assert.Throws<ArgumentException>(() => { var c = Valid(); c.AllowHttp2 = true; c.Validate(); });
            Assert.Throws<ArgumentException>(() => { var c = Valid(); c.InboundPort = 0; c.Validate(); });
            Assert.Throws<ArgumentException>(() => { var c = Valid(); c.OutboundPort = 70000; c.Validate(); });

            //TCP endpoints are not affected.
            new EndpointConfiguration { Name = "tcp", InboundTrafficType = NtTrafficType.Https, OutboundTrafficType = NtTrafficType.Http }.Validate();
        }

        [Fact]
        public void Configuration_ShowsUdpAsAName_InTheFiles()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new EndpointConfiguration
            {
                InboundTrafficType = NtTrafficType.Udp,
                OutboundTrafficType = NtTrafficType.Udp
            });

            Assert.Contains("\"InboundTrafficType\":\"Udp\"", json);
            Assert.DoesNotContain("IsUdp", json);

            var loaded = System.Text.Json.JsonSerializer.Deserialize<EndpointConfiguration>(json);
            Assert.True(loaded!.IsUdp);
        }
    }
}
