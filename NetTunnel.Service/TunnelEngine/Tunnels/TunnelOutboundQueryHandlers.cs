using NetTunnel.Service.ReliableMessages.Query;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal class TunnelOutboundQueryHandlers : TunnelMessageHandlerBase, IRmMessageHandler
    {
        public QueryReplyPayloadBoolean OnQueryAddEndpointInbound(RmContext context, QueryAddEndpointInbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.AddInboundEndpoint(query.Configuration);
            endpoint.Start();
            return new QueryReplyPayloadBoolean(true);
        }

        public QueryReplyPayloadBoolean OnQueryAddEndpointOutbound(RmContext context, QueryAddEndpointOutbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.AddOutboundEndpoint(query.Configuration);
            endpoint.Start();
            return new QueryReplyPayloadBoolean(true);
        }

        public QueryReplyPayloadBoolean OnQueryDeleteEndpoint(RmContext context, QueryDeleteEndpoint query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.DeleteEndpoint(query.EndpointId);
            return new QueryReplyPayloadBoolean(true);
        }
    }
}
