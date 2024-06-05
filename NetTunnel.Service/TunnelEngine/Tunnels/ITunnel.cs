using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal interface ITunnel
    {
        public TunnelEngineCore Core { get; }

        public bool KeepRunning { get; }
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service tunnel has the same id.
        /// </summary>
        public Guid TunnelId { get; }
        public string Name { get; }
        //public NASCCLStream? NascclStream { get; }

        //public IRmEndpoint ReliableEndpoint { get; }

        public byte[]? EncryptionKey { get; }
        public bool SecureKeyExchangeIsComplete { get; }

        public void Start();
        public void Stop();

        public Task<T> Query<T>(IRmQuery<T> query) where T : IRmQueryReply;
        public void Notify(IRmNotification notification);

        internal List<IEndpoint> Endpoints { get; set; }

        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; }
        public ulong CurrentConnections { get; }
        public NtTunnelStatus Status { get; set; }
    }
}
