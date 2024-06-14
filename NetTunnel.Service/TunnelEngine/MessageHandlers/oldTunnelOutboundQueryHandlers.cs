using NetTunnel.Service.ReliableMessages.Query;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.MessageHandlers
{
    internal class oldTunnelOutboundQueryHandlers : oldTunnelMessageHandlerBase, IRmMessageHandler
    {
        /*
        /// <summary>
        /// The remote service is asking us to add an inbound endpoint to this tunnel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public oldQueryReplyPayloadBoolean OnQueryUpsertEndpointInbound(RmContext context, oldQueryUpsertEndpointInbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.UpsertInboundEndpoint(query.Configuration);
            endpoint.Start();
            return new oldQueryReplyPayloadBoolean(true);
        }

        /// <summary>
        /// The remote service is asking us to add an outbound endpoint to this tunnel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public oldQueryReplyPayloadBoolean OnQueryUpsertEndpointOutbound(RmContext context, oldQueryUpsertEndpointOutbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.UpsertOutboundEndpoint(query.Configuration);
            endpoint.Start();
            return new oldQueryReplyPayloadBoolean(true);
        }

        /// <summary>
        /// The remote service is asking us to delete an endpoint, from this tunnel, based on the endpoint id.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public oldQueryReplyPayloadBoolean OnQueryDeleteEndpoint(RmContext context, oldQueryDeleteEndpoint query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.DeleteEndpoint(query.EndpointId);
            return new oldQueryReplyPayloadBoolean(true);
        }
        */
    }
}
