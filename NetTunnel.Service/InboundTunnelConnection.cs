using NetTunnel.Service.ReliableMessages;

namespace NetTunnel.Service
{
    internal class InboundTunnelConnection
    {
        public Guid ConnectionId { get; set; }

        public InboundTunnelConnection(Guid connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
