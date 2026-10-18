using NetTunnel.Library;
using NTDLS.Helpers;
using NTDLS.ReliableMessaging;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    internal class EndpointEdgeConnection : IDisposable
    {
        private Stream _stream;

        public bool IsConnected { get; private set; }
        public DateTime LastActivityDateTime { get; private set; } = DateTime.UtcNow;
        public DateTime StartDateTime { get; private set; } = DateTime.UtcNow;
        public Guid EdgeId { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public Thread Thread { get; private set; }
        public ulong BytesReceived { get; internal set; }
        public ulong BytesSent { get; internal set; }
        public RmSequenceBuffer<byte[]> SequenceBuffer { get; internal set; } = new();

        public double ActivityAgeInMilliseconds
            => (DateTime.UtcNow - LastActivityDateTime).TotalMilliseconds;

        public double StartAgeInMilliseconds
            => (DateTime.UtcNow - StartDateTime).TotalMilliseconds;

        public EndpointEdgeConnection(Thread thread, TcpClient tcpClient, Guid edgeId)
        {
            Thread = thread;
            TcpClient = tcpClient;
            EdgeId = edgeId;
            _stream = tcpClient.GetStream();
            IsConnected = true;
        }

        /// <summary>
        /// How long a TLS handshake may take before the connection is abandoned. Prevents a client
        ///     that connects and never completes the handshake from holding a thread indefinitely.
        /// </summary>
        private static readonly TimeSpan TlsHandshakeTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Terminates TLS for a client that connected to us (e.g. a web-browser). Blocks until the handshake completes or times out.
        /// </summary>
        public void AuthenticateAsTlsServer(X509Certificate2 certificate)
        {
            var sslStream = new SslStream(_stream, false);

            using var cts = new CancellationTokenSource(TlsHandshakeTimeout);
            sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
            {
                ServerCertificate = certificate,
                ClientCertificateRequired = false,
                EnabledSslProtocols = SslProtocols.None, //Let the OS choose.
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck
            }, cts.Token).GetAwaiter().GetResult();

            _stream = sslStream;
        }

        /// <summary>
        /// Starts TLS to a server that we connected to (e.g. a web-server). Blocks until the handshake completes or times out.
        /// </summary>
        public void AuthenticateAsTlsClient(string targetHost, bool ignoreCertificateErrors)
        {
            var sslStream = new SslStream(_stream, false);

            var options = new SslClientAuthenticationOptions
            {
                TargetHost = targetHost
            };

            if (ignoreCertificateErrors)
            {
                options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            }

            using var cts = new CancellationTokenSource(TlsHandshakeTimeout);
            sslStream.AuthenticateAsClientAsync(options, cts.Token).GetAwaiter().GetResult();

            _stream = sslStream;
        }

        public void Disconnect()
        {
            Exceptions.Ignore(_stream.Close);
            Exceptions.Ignore(TcpClient.Close);
            IsConnected = false;
        }

        public void Write(long packetSequence, byte[] buffer)
        {
            SequenceBuffer.Process(buffer, packetSequence,
                (data) => _stream.Write(data));

            BytesSent += (ulong)buffer.Length;
            LastActivityDateTime = DateTime.UtcNow;
        }

        public bool Read(ref ReceiveBuffer buffer)
        {
            LastActivityDateTime = DateTime.UtcNow;
            buffer.Length = _stream.Read(buffer.Bytes, 0, buffer.Bytes.Length);
            BytesReceived += (ulong)buffer.Length;
            return buffer.Length > 0;
        }

        public void Dispose()
        {
            Disconnect();

            Exceptions.Ignore(_stream.Dispose);
            Exceptions.Ignore(TcpClient.Dispose);
            Exceptions.Ignore(SequenceBuffer.Clear);
        }
    }
}
