using NetTunnel.Library.Types;
using NetTunnel.Service.TunnelEngine.Endpoints;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine
{
    internal interface ITunnel
    {
        public TunnelConfiguration Configuration { get; }
        public NtTunnelStatus Status { get; set; }
        public ulong BytesReceived { get; set; }
        public ulong BytesSent { get; set; }
        public ulong TotalConnections { get; set; }
        public ulong CurrentConnections { get; set; }
        public ServiceEngine ServiceEngine { get; }
        public List<IEndpoint> Endpoints { get; }

        public void Start();
        public void Stop();

        public EndpointInbound UpsertEndpoint(EndpointConfiguration configuration);
        public void DeleteEndpoint(Guid endpointId);
        public TunnelConfiguration CloneConfiguration();

        public void SendNotificationOfEndpointDataExchange(Guid tunnelId, Guid endpointId, Guid streamId, byte[] bytes, int length);
        public void SendNotificationOfEndpointConnect(Guid tunnelId, Guid endpointId, Guid streamId);
    }
}
