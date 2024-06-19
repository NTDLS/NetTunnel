using NetTunnel.Library.Types;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    public interface IEndpoint
    {
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service endpoint has the same id.
        /// </summary>
        public Guid EndpointId { get; }
        public bool KeepRunning { get; }
        public void Disconnect(Guid streamId);
        public void SendEndpointData(Guid streamId, byte[] buffer);
        public void Start();
        public void Stop();
        public ulong BytesReceived { get; }
        public ulong BytesSent { get; }
        public ulong TotalConnections { get; }
        public ulong CurrentConnections { get; }
        public EndpointConfiguration Configuration { get; }
        public NtDirection Direction { get; }
        /// <summary>
        /// Unique ID that takes the direction and the ID into account.
        /// </summary>
        public DirectionalKey EndpointKey { get; }
    }
}
