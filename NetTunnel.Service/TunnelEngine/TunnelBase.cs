using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    /// <summary>
    /// This is the class that makes an outbound TCP/IP connection to a listening tunnel service.
    /// </summary>
    internal class TunnelBase : ITunnel
    {
        /// <summary>
        /// Returns Outbound if the tunnel is owned by the local service, otherwise returns Inbound.
        /// </summary>
        public NtDirection Direction
                => Configuration.ServiceId == Singletons.Configuration.ServiceId ? NtDirection.Outbound : NtDirection.Inbound;

        public override int GetHashCode()
        {
            return Configuration.TunnelId.GetHashCode()
                + Configuration.Name.GetHashCode()
                + Endpoints.Sum(o => o.GetHashCode());
        }

        public int ChangeHash
            => Configuration.TunnelId.GetHashCode()
            + Configuration.Name.GetHashCode();

        #region Public Properties.

        public NtTunnelConfiguration Configuration { get; private set; }

        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; set; }
        public ulong CurrentConnections { get; set; }
        public ServiceEngine Core { get; private set; }
        public bool KeepRunning { get; private set; } = false;
        //public Guid TunnelId { get; private set; }
        //public string Name { get; private set; }
        public List<IEndpoint> Endpoints { get; private set; } = new();
        private Thread? _heartbeatThread;

        #endregion

        public TunnelBase(ServiceEngine core, NtTunnelConfiguration configuration)
        {
            Core = core;
            Configuration = configuration.CloneConfiguration();

            Configuration.Endpoints.Where(o => o.Direction == NtDirection.Inbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointInbound(Core, this, o)));

            Configuration.Endpoints.Where(o => o.Direction == NtDirection.Outbound)
                .ToList().ForEach(o => Endpoints.Add(new EndpointOutbound(Core, this, o)));

        }

        public IEndpoint? GetEndpointById(Guid pairId)
            => Endpoints.Where(o => o.EndpointId == pairId).SingleOrDefault();

        public NtTunnelConfiguration CloneConfiguration()
        {
            return Configuration.CloneConfiguration();
        }

        public virtual void Start()
        {
            if (KeepRunning == true)
            {
                return;
            }
            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Starting tunnel '{Configuration.Name}'.");

            KeepRunning = true;

            _heartbeatThread = new Thread(HeartbeatThreadProc);
            _heartbeatThread.Start();

            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Starting endpoints for tunnel '{Configuration.Name}'.");

            Endpoints.ForEach(x => x.Start());
        }

        public virtual void Stop()
        {
            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Stopping tunnel '{Configuration.Name}'.");

            Endpoints.ForEach(o => o.Stop());

            KeepRunning = false;
            _heartbeatThread?.Join();

            Status = NtTunnelStatus.Stopped;

            Core.Logging.Write(NtLogSeverity.Verbose,
                $"Stopped tunnel '{Configuration.Name}'.");
        }

        public void SendEndpointData(Guid endpointId, Guid StreamId, byte[] bytes)
        {
            Endpoints.Where(o=>o.EndpointId == endpointId)
                .Single().SendEndpointData(StreamId, bytes);
        }

        #region Add/Delete Endpoints.

        public EndpointInbound UpsertEndpoint(NtEndpointConfiguration configuration)
        {
            var existingEndpoint = GetEndpointById(configuration.EndpointId);
            if (existingEndpoint != null)
            {
                DeleteEndpoint(existingEndpoint.EndpointId);
            }

            var endpoint = new EndpointInbound(Core, this, configuration);
            Configuration.Endpoints.Add(configuration);
            Endpoints.Add(endpoint);
            return endpoint;
        }

        public void DeleteEndpoint(Guid endpointId)
        {
            var endpoint = GetEndpointById(endpointId);
            if (endpoint != null)
            {
                endpoint.Stop();
                Configuration.Endpoints.RemoveAll(o => o.EndpointId == endpointId);
                Endpoints.Remove(endpoint);
            }
        }

        #endregion

        private void HeartbeatThreadProc()
        {
            Thread.CurrentThread.Name = $"HeartbeatThreadProc:{Environment.CurrentManagedThreadId}";

            DateTime lastHeartBeat = DateTime.UtcNow;

            while (KeepRunning)
            {
                if ((DateTime.UtcNow - lastHeartBeat).TotalMilliseconds > Singletons.Configuration.TunnelAndEndpointHeartbeatDelayMs)
                {
                    //var pingTime = _client.Ping();
                    //Core.Logging.Write(NtLogSeverity.Debug,
                    //    $"Roundtrip time for '{Configuration.Name}': {pingTime:n0}ms"); ;

                    lastHeartBeat = DateTime.UtcNow;
                }

                Thread.Sleep(100);
            }
        }

        public virtual void NotificationEndpointExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length)
            => throw new NotImplementedException("This function should be overridden.");

        public virtual void NotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId)
            => throw new NotImplementedException("This function should be overridden.");
    }
}
