using NetTunnel.Service.ReliableMessages.Query;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.Tunnels.MessageHandlers
{
    internal class TunnelOutboundQueryHandlers : TunnelMessageHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// The remote service is asking us to add an inbound endpoint to this tunnel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryReplyPayloadBoolean OnQueryUpsertEndpointInbound(RmContext context, QueryUpsertEndpointInbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.UpsertInboundEndpoint(query.Configuration);
            endpoint.Start();
            return new QueryReplyPayloadBoolean(true);
        }

        /// <summary>
        /// The remote service is asking us to add an outbound endpoint to this tunnel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryReplyPayloadBoolean OnQueryUpsertEndpointOutbound(RmContext context, QueryUpsertEndpointOutbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.UpsertOutboundEndpoint(query.Configuration);
            endpoint.Start();
            return new QueryReplyPayloadBoolean(true);
        }

        /// <summary>
        /// The remote service is asking us to delete an endpoint, from this tunnel, based on the endpoint id.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryReplyPayloadBoolean OnQueryDeleteEndpoint(RmContext context, QueryDeleteEndpoint query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.DeleteEndpoint(query.EndpointId);
            return new QueryReplyPayloadBoolean(true);
        }
    }
}
