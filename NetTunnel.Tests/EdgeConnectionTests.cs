using System.Net;
using System.Net.Sockets;
using NetTunnel.Service.TunnelEngine.Endpoints;

namespace NetTunnel.Tests
{
    /// <summary>
    /// The receiving end of a tunnel edge gets numbered packets from concurrent, unordered notification handlers and must put exactly
    /// the sent bytes on the socket, in order, and only then close it. These tests use real loopback sockets.
    /// </summary>
    public class EdgeConnectionTests
    {
        private static (EndpointEdgeConnection Edge, TcpClient Peer) Connect(TimeSpan? gracePeriod = null)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var peer = new TcpClient();
            peer.Connect((IPEndPoint)listener.LocalEndpoint);
            var accepted = listener.AcceptTcpClient();
            listener.Stop();

            var edge = new EndpointEdgeConnection(new Thread(() => { }), accepted, Guid.NewGuid())
            {
                DisconnectGracePeriod = gracePeriod ?? TimeSpan.FromSeconds(15)
            };
            return (edge, peer);
        }

        /// <summary>Reads until the edge closes the connection.</summary>
        private static Task<byte[]> ReadToEnd(TcpClient peer) => Task.Run(() =>
        {
            var received = new MemoryStream();
            var buffer = new byte[8192];
            int read;
            while ((read = peer.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            {
                received.Write(buffer, 0, read);
            }
            return received.ToArray();
        });

        private static List<byte[]> Packets(int count, int seed)
        {
            var random = new Random(seed);
            var packets = new List<byte[]>();
            for (int i = 0; i < count; i++)
            {
                var packet = new byte[random.Next(1, 6000)];
                random.NextBytes(packet);
                packets.Add(packet);
            }
            return packets;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void ConcurrentUnorderedWrites_ThenDisconnect_DeliverEveryByteInOrderBeforeClosing(int seed)
        {
            const int count = 400;
            var (edge, peer) = Connect();
            var packets = Packets(count, seed);
            var expected = packets.SelectMany(o => o).ToArray();
            var received = ReadToEnd(peer);

            //Hand the packets to several threads in a scrambled order, like the notification handlers do.
            var order = Enumerable.Range(0, count).OrderBy(i => (i * 7919 + seed) % 1013).ToList();
            var queue = new System.Collections.Concurrent.ConcurrentQueue<int>(order);
            var disconnectAfterSomeWritten = new ManualResetEventSlim();
            int written = 0;

            var writers = Enumerable.Range(0, 6).Select(_ => Task.Run(() =>
            {
                while (queue.TryDequeue(out var sequence))
                {
                    edge.Write(sequence, packets[sequence]);
                    if (Interlocked.Increment(ref written) == 50)
                    {
                        disconnectAfterSomeWritten.Set();
                    }
                }
            })).ToArray();

            //The disconnect is handled while the packets before it are still being written.
            disconnectAfterSomeWritten.Wait();
            edge.DisconnectAfter(count);

            Task.WaitAll(writers);
            Assert.True(received.Wait(TimeSpan.FromSeconds(30)), "the connection was never closed");
            Assert.Equal(expected.Length, received.Result.Length);
            Assert.True(expected.AsSpan().SequenceEqual(received.Result), "the bytes differ");
            Assert.False(edge.IsConnected);
        }

        [Fact]
        public void DisconnectBeforeAnyPacketArrives_WaitsForThemAll()
        {
            var (edge, peer) = Connect();
            var packets = Packets(20, 99);
            var received = ReadToEnd(peer);

            edge.DisconnectAfter(packets.Count);
            Assert.True(edge.IsConnected);

            for (int i = packets.Count - 1; i >= 0; i--) //Last packet first.
            {
                Assert.True(edge.IsConnected);
                edge.Write(i, packets[i]);
            }

            Assert.True(received.Wait(TimeSpan.FromSeconds(10)));
            Assert.True(packets.SelectMany(o => o).SequenceEqual(received.Result));
            Assert.False(edge.IsConnected);
        }

        [Fact]
        public void DisconnectAfterEverythingWasDelivered_ClosesAtOnce()
        {
            var (edge, peer) = Connect();
            var packets = Packets(5, 7);
            var received = ReadToEnd(peer);

            for (int i = 0; i < packets.Count; i++)
            {
                edge.Write(i, packets[i]);
            }
            Assert.True(edge.IsConnected);

            edge.DisconnectAfter(packets.Count);

            Assert.False(edge.IsConnected);
            Assert.True(received.Wait(TimeSpan.FromSeconds(10)));
            Assert.True(packets.SelectMany(o => o).SequenceEqual(received.Result));
        }

        [Fact]
        public void UnknownPacketCount_ClosesAtOnce()
        {
            var (edge, peer) = Connect();
            var received = ReadToEnd(peer);

            edge.DisconnectAfter(-1);

            Assert.False(edge.IsConnected);
            Assert.True(received.Wait(TimeSpan.FromSeconds(10)));
            Assert.Empty(received.Result);
        }

        [Fact]
        public void PacketThatNeverArrives_DoesNotHoldTheConnectionOpenForever()
        {
            var (edge, peer) = Connect(TimeSpan.FromMilliseconds(300));
            var received = ReadToEnd(peer);
            var packets = Packets(3, 11);

            edge.Write(0, packets[0]);
            edge.Write(2, packets[2]); //Packet 1 is lost.
            edge.DisconnectAfter(3);

            Assert.True(received.Wait(TimeSpan.FromSeconds(10)), "the connection stayed open");
            Assert.False(edge.IsConnected);
            Assert.True(packets[0].SequenceEqual(received.Result)); //What could be delivered in order was.
        }
    }
}
