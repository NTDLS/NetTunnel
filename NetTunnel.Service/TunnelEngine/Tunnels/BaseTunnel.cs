using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.NASCCL;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal class BaseTunnel : ITunnel
    {
        public byte[]? EncryptionKey { get; protected set; }
        public bool SecureKeyExchangeIsComplete { get; protected set; }
        public NASCCLStream? NascclStream { get; protected set; }

        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; internal set; }
        public ulong CurrentConnections { get; internal set; }
        public TunnelEngineCore Core { get; private set; }
        public bool KeepRunning { get; internal set; } = false;
        public Guid TunnelId { get; private set; }
        public string Name { get; private set; }

        public List<IEndpoint> Endpoints { get; set; } = new();

        private readonly Thread _heartbeatThread;

        public BaseTunnel(TunnelEngineCore core, NtTunnelInboundConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        public BaseTunnel(TunnelEngineCore core, NtTunnelOutboundConfiguration configuration)
        {
            Core = core;

            TunnelId = configuration.TunnelId;
            Name = configuration.Name;

            configuration.EndpointInboundConfigurations.ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));
            configuration.EndpointOutboundConfigurations.ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();
        }

        public virtual void Start()
        {
            KeepRunning = true;
        }

        public virtual void Stop()
        {
            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread.Join();
        }

        private void HeartbeatThreadProc()
        {
            Thread.CurrentThread.Name = $"HeartbeatThreadProc:{Environment.CurrentManagedThreadId}";

            DateTime lastHeartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastHeartBeat).TotalMilliseconds > Singletons.Configuration.HeartbeatDelayMs)
                {
                    lastHeartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }

        public IEndpoint? GetEndpointById(Guid pairId) => Endpoints.Where(o => o.EndpointId == pairId).FirstOrDefault();

        #region Endpoint CRUD helpers.

        public EndpointInbound AddInboundEndpoint(NtEndpointInboundConfiguration configuration)
        {
            var endpoint = new EndpointInbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
            return endpoint;
        }

        public EndpointOutbound AddOutboundEndpoint(NtEndpointOutboundConfiguration configuration)
        {
            var endpoint = new EndpointOutbound(Core, this, configuration);
            Endpoints.Add(endpoint);
            if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
            if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointPairId)
        {
            var endpoint = Endpoints.Where(o => o.EndpointId == endpointPairId).SingleOrDefault();
            if (endpoint != null)
            {
                endpoint.Stop();
                Endpoints.Remove(endpoint);
                if (this is TunnelInbound) Core.InboundTunnels.SaveToDisk();
                if (this is TunnelOutbound) Core.OutboundTunnels.SaveToDisk();
            }
        }

        #endregion

        #region Reliable Messaging Passthrough.

        public virtual Task<T> Query<T>(IRmQuery<T> query) where T : class, IRmQueryReply
            => throw new Exception("Query<T>() must be overridden.");

        public virtual void Notify(IRmNotification notification)
            => throw new Exception("Notify() must be overridden.");

        #endregion
    }
}
