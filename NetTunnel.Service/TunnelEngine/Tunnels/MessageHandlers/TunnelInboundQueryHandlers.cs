using NetTunnel.Service.ReliableMessages.Query;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Service.TunnelEngine.Tunnels.MessageHandlers
{
    internal class TunnelInboundQueryHandlers : TunnelMessageHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// The remote service has made an outgoing tunnel connection and has started the process of exchanging a key.
        /// Here we need to apply the diffie–hellman negation token and reply with the diffie–hellman reply token
        /// We will then "Initialize the Cryptography Provider" but will not use apply (use it) until the remote service
        /// sends the NotificationApplyCryptography notification. This is because if we apply the cryptography now, then
        /// the reply will be encrypted before the remote service has the data it needs to decrypt it.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryReplyKeyExchangeReply OnQueryRequestKeyExchange(RmContext context, QueryRequestKeyExchange query)
        {
            var tunnel = GetTunnel<TunnelInbound>(context);

            var compoundNegotiator = new CompoundNegotiator();
            var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
            var negotiationReply = new QueryReplyKeyExchangeReply(negotiationReplyToken);

            tunnel.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

            return negotiationReply;
        }

        /// <summary>
        /// The remote service is asking us to add an inbound endpoint to this tunnel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryReplyPayloadBoolean OnQueryAddEndpointInbound(RmContext context, QueryAddEndpointInbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            var endpoint = tunnel.AddInboundEndpoint(query.Configuration);
            endpoint.Start();
            return new QueryReplyPayloadBoolean(true);
        }

        /// <summary>
        /// The remote service is asking us to add an outbound endpoint to this tunnel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryReplyPayloadBoolean OnQueryAddEndpointOutbound(RmContext context, QueryAddEndpointOutbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            var endpoint = tunnel.AddOutboundEndpoint(query.Configuration);
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
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            tunnel.DeleteEndpoint(query.EndpointId);
            return new QueryReplyPayloadBoolean(true);
        }
    }
}
