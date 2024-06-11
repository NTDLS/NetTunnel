using NetTunnel.Service.ReliableMessages.Query;
using NetTunnel.Service.ReliableMessages.Query.Reply;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal class TunnelInboundQueryHandlers : TunnelMessageHandlerBase, IRmMessageHandler
    {
        public QueryReplyKeyExchangeReply OnQueryRequestKeyExchange(RmContext context, QueryRequestKeyExchange query)
        {
            var tunnel = GetTunnel<TunnelInbound>(context);

            //We received a diffie–hellman key exchange request, respond to it so we can prop up encryption.
            var compoundNegotiator = new CompoundNegotiator();
            var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
            var negotiationReply = new QueryReplyKeyExchangeReply(negotiationReplyToken);

            tunnel.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

            return negotiationReply;
        }

        public QueryReplyPayloadBoolean OnQueryAddEndpointInbound(RmContext context, QueryAddEndpointInbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            var endpoint = tunnel.AddInboundEndpoint(query.Configuration);
            endpoint.Start();
            return new QueryReplyPayloadBoolean(true);
        }

        public QueryReplyPayloadBoolean OnQueryAddEndpointOutbound(RmContext context, QueryAddEndpointOutbound query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            var endpoint = tunnel.AddOutboundEndpoint(query.Configuration);
            endpoint.Start();
            return new QueryReplyPayloadBoolean(true);
        }

        public QueryReplyPayloadBoolean OnQueryDeleteEndpoint(RmContext context, QueryDeleteEndpoint query)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            tunnel.DeleteEndpoint(query.EndpointId);
            return new QueryReplyPayloadBoolean(true);
        }
    }
}
