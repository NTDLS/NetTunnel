using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    internal interface ITunnel
    {
        public NtTunnelConfiguration Configuration { get; }
        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; set; }
        public ulong CurrentConnections { get; set; }
        public ServiceEngine Core { get; }
        public List<IEndpoint> Endpoints { get; }

        public void Start();
        public void Stop();

        public EndpointInbound UpsertEndpoint(NtEndpointConfiguration configuration);
        public void DeleteEndpoint(Guid endpointId);
        public NtTunnelConfiguration CloneConfiguration();

        public void NotificationEndpointExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length);
        public void NotificationEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId);
    }
}
