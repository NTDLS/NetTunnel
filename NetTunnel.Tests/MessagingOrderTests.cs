using System.Net;
using System.Net.Sockets;
using NetTunnel.Library.Payloads;
using NetTunnel.Library.ReliablePayloads.Notification.ServiceToService;
using NTDLS.ReliableMessaging;
using Xunit.Abstractions;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Tests
{
    /// <summary>
    /// The messaging layer handles the notifications of one connection concurrently and not in the order they were sent. The tunnel
    /// therefore numbers the data packets of an edge (see EndpointEdgeConnection) and a Disconnect must say how many packets precede
    /// it. These tests send real tunnel payloads over loopback with the same configuration the service uses.
    /// </summary>
    public class MessagingOrderTests(ITestOutputHelper output)
    {
        private class Recorder : IRmMessageHandler
        {
            public readonly List<string> Events = [];
            public readonly object Lock = new();
            public int Concurrent, MaxConcurrent;
            public readonly ManualResetEventSlim Done = new();
            public int Expected;

            public void OnNotify(RmContext context, S2SNotificationEndpointDataExchange notification)
            {
                int now = Interlocked.Increment(ref Concurrent);
                lock (Lock) { MaxConcurrent = Math.Max(MaxConcurrent, now); }
                Thread.SpinWait(200); //A little work, so that overlapping handlers would be seen.
                if (notification.PacketSequence % 50 == 0)
                {
                    Thread.Sleep(5); //A socket write to a slow client blocks like this.
                }
                lock (Lock) { Events.Add($"data:{notification.PacketSequence}:{notification.Bytes.Length}"); }
                Interlocked.Decrement(ref Concurrent);
                Check();
            }

            public void OnNotify(RmContext context, S2SNotificationEndpointDisconnect notification)
            {
                lock (Lock) { Events.Add("disconnect"); }
                Check();
            }

            private void Check()
            {
                lock (Lock)
                {
                    if (Events.Count >= Expected)
                    {
                        Done.Set();
                    }
                }
            }
        }

        private static int FreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static RmConfiguration Configuration() => new()
        {
            InitialReceiveBufferSize = 16384,
            MaxReceiveBufferSize = 1048576,
            ReceiveBufferGrowthRate = 0.2
        };

        [Fact]
        public void Notifications_AreConcurrentAndUnordered_ButNeverLost()
        {
            const int packets = 3000;
            var recorder = new Recorder { Expected = packets + 1 };
            int port = FreePort();

            var server = new RmServer(Configuration());
            server.SetCompressionProvider(new RmDeflateCompressionProvider());
            server.AddHandler(recorder);
            server.Start(port);

            var client = new RmClient(Configuration());
            client.SetCompressionProvider(new RmDeflateCompressionProvider());
            try
            {
                client.Connect("127.0.0.1", port);

                var key = new DirectionalKey(Guid.NewGuid(), NtDirection.Outbound);
                var endpoint = Guid.NewGuid();
                var edge = Guid.NewGuid();
                var random = new Random(7);

                //Several sender threads must not reorder what one sender sent; here one pump sends, like the tunnel does.
                for (int i = 0; i < packets; i++)
                {
                    var payload = new byte[random.Next(1, 20000)];
                    client.Notify(new S2SNotificationEndpointDataExchange(key, endpoint, edge, i, payload, payload.Length));
                }
                client.Notify(new S2SNotificationEndpointDisconnect(key, endpoint, edge));

                Assert.True(recorder.Done.Wait(TimeSpan.FromSeconds(60)), $"only {recorder.Events.Count} of {packets + 1} notifications arrived");

                lock (recorder.Lock)
                {
                    int disconnectAt = recorder.Events.IndexOf("disconnect");
                    int outOfOrder = 0;
                    for (int i = 1; i < packets; i++)
                    {
                        if (Sequence(recorder.Events.Where(o => o.StartsWith("data")).ElementAt(i)) < Sequence(recorder.Events.Where(o => o.StartsWith("data")).ElementAt(i - 1)))
                        {
                            outOfOrder++;
                        }
                    }
                    output.WriteLine($"max handlers running at once: {recorder.MaxConcurrent}; data packets handled out of order: {outOfOrder}");
                    output.WriteLine($"disconnect handled at position {disconnectAt} of {recorder.Events.Count}; {recorder.Events.Count - 1 - disconnectAt} data packets handled after it");

                    //What the tunnel must be built for: delivery is concurrent and not ordered, but nothing is lost or repeated.
                    Assert.Equal(packets, recorder.Events.Where(o => o.StartsWith("data")).Select(Sequence).Distinct().Count());
                    Assert.Single(recorder.Events, "disconnect");
                }
            }
            finally
            {
                Exceptions(() => client.Disconnect());
                Exceptions(() => server.Stop());
            }
        }

        private static long Sequence(string dataEvent) => long.Parse(dataEvent.Split(':')[1]);

        private static void Exceptions(Action action)
        {
            try { action(); } catch { /* best effort cleanup */ }
        }
    }
}
