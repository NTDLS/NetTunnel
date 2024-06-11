using NetTunnel.Library;
using NetTunnel.Service.FramePayloads.Queries;
using NetTunnel.Service.FramePayloads.Replies;
using NTDLS.ReliableMessaging;
using NTDLS.SecureKeyExchange;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    public class TunnelInboundQueryHandlers : IRmMessageHandler
    {
        private static TunnelInbound EnforceCryptography(RmContext context)
        {
            var inboundTunnel = (context.Endpoint.Parameter as TunnelInbound).EnsureNotNull();
            if (!inboundTunnel.SecureKeyExchangeIsComplete)
            {
                throw new Exception("Cryptography has not been initialized.");
            }
            return inboundTunnel;
        }

        public NtFramePayloadKeyExchangeReply OnNtFramePayloadRequestKeyExchange(RmContext context, NtFramePayloadRequestKeyExchange query)
        {
            var inboundTunnel = (context.Endpoint.Parameter as TunnelInbound).EnsureNotNull();

            //We received a diffie–hellman key exchange request, respond to it so we can prop up encryption.
            var compoundNegotiator = new CompoundNegotiator();
            var negotiationReplyToken = compoundNegotiator.ApplyNegotiationToken(query.NegotiationToken);
            var negotiationReply = new NtFramePayloadKeyExchangeReply(negotiationReplyToken);

            inboundTunnel.InitializeCryptographyProvider(compoundNegotiator.SharedSecret);

            return negotiationReply;
        }

        public NtFramePayloadBoolean OnNtFramePayloadAddEndpointInbound(RmContext context, NtFramePayloadAddEndpointInbound query)
        {
            var inboundTunnel = EnforceCryptography(context);

            var endpoint = inboundTunnel.AddInboundEndpoint(query.Configuration);
            endpoint.Start();
            return new NtFramePayloadBoolean(true);
        }

        public NtFramePayloadBoolean OnNtFramePayloadAddEndpointOutbound(RmContext context, NtFramePayloadAddEndpointOutbound query)
        {
            var inboundTunnel = EnforceCryptography(context);

            var endpoint = inboundTunnel.AddOutboundEndpoint(query.Configuration);
            endpoint.Start();
            return new NtFramePayloadBoolean(true);
        }

        public NtFramePayloadBoolean OnNtFramePayloadDeleteEndpoint(RmContext context, NtFramePayloadDeleteEndpoint query)
        {
            var inboundTunnel = EnforceCryptography(context);

            inboundTunnel.DeleteEndpoint(query.EndpointId);
            return new NtFramePayloadBoolean(true);
        }
    }
}
