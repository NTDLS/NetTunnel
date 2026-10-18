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

        /// <summary>
        /// The application protocol (ALPN) used on this connection: "h2" or "http/1.1", or null when it was not negotiated.
        ///     Inbound: what the client negotiated with us. Outbound: what the inbound end told us to ask the destination for.
        /// </summary>
        public string? ApplicationProtocol { get; internal set; }

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
        public void AuthenticateAsTlsServer(X509Certificate2 certificate, bool offerHttp2 = false)
        {
            var sslStream = new SslStream(_stream, false);

            using var cts = new CancellationTokenSource(TlsHandshakeTimeout);
            sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
            {
                ServerCertificate = certificate,
                ApplicationProtocols = offerHttp2
                    ? [SslApplicationProtocol.Http2, SslApplicationProtocol.Http11]
                    : [SslApplicationProtocol.Http11],
                ClientCertificateRequired = false,
                EnabledSslProtocols = SslProtocols.None, //Let the OS choose.
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck
            }, cts.Token).GetAwaiter().GetResult();

            _stream = sslStream;
            ApplicationProtocol = ProtocolName(sslStream.NegotiatedApplicationProtocol);
        }

        /// <summary>
        /// Starts TLS to a server that we connected to (e.g. a web-server). Blocks until the handshake completes or times out.
        /// </summary>
        public void AuthenticateAsTlsClient(string targetHost, bool ignoreCertificateErrors, string? applicationProtocol = null)
        {
            var sslStream = new SslStream(_stream, false);

            var options = new SslClientAuthenticationOptions
            {
                TargetHost = targetHost,
                ApplicationProtocols = applicationProtocol == null ? null
                    : [applicationProtocol == "h2" ? SslApplicationProtocol.Http2 : SslApplicationProtocol.Http11]
            };

            if (ignoreCertificateErrors)
            {
                options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
            }

            using var cts = new CancellationTokenSource(TlsHandshakeTimeout);
            try
            {
                sslStream.AuthenticateAsClientAsync(options, cts.Token).GetAwaiter().GetResult();
            }
            catch (AuthenticationException ex) when (applicationProtocol == "h2")
            {
                //A destination that does not do HTTP/2 refuses the handshake, with a message that does not say why.
                throw new AuthenticationException($"The client is using HTTP/2 and the TLS handshake with '{targetHost}' failed. This usually means the"
                    + $" destination does not support HTTP/2: turn off HTTP/2 on the endpoint, or enable it on the destination. ({ex.InnerException?.Message ?? ex.Message})", ex);
            }

            _stream = sslStream;

            var negotiated = ProtocolName(sslStream.NegotiatedApplicationProtocol);
            if (applicationProtocol == "h2" && negotiated != "h2")
            {
                throw new AuthenticationException($"The client is using HTTP/2 but the destination '{targetHost}' did not agree to it"
                    + " (it negotiated " + (negotiated ?? "nothing") + "). Turn off HTTP/2 on the endpoint, or enable it on the destination.");
            }
        }

        private static string? ProtocolName(SslApplicationProtocol protocol)
            => protocol == SslApplicationProtocol.Http2 ? "h2" : protocol == SslApplicationProtocol.Http11 ? "http/1.1" : null;

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
