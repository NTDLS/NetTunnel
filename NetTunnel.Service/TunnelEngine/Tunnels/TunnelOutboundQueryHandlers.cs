using NetTunnel.Library;
using NetTunnel.Service.FramePayloads.Queries;
using NetTunnel.Service.FramePayloads.Replies;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    public class TunnelOutboundQueryHandlers : IRmMessageHandler
    {
        private static TunnelOutbound EnforceCryptography(RmContext context)
        {
            var outboundTunnel = (context.Endpoint.Parameter as TunnelOutbound).EnsureNotNull();
            if (!outboundTunnel.SecureKeyExchangeIsComplete)
            {
                throw new Exception("Cryptography has not been initialized.");
            }
            return outboundTunnel;
        }

        public NtFramePayloadBoolean OnNtFramePayloadAddEndpointInbound(RmContext context, NtFramePayloadAddEndpointInbound query)
        {
            var outboundTunnel = EnforceCryptography(context);

            var endpoint = outboundTunnel.AddInboundEndpoint(query.Configuration);
            endpoint.Start();
            return new NtFramePayloadBoolean(true);
        }

        public NtFramePayloadBoolean OnNtFramePayloadAddEndpointOutbound(RmContext context, NtFramePayloadAddEndpointOutbound query)
        {
            var outboundTunnel = EnforceCryptography(context);

            var endpoint = outboundTunnel.AddOutboundEndpoint(query.Configuration);
            endpoint.Start();
            return new NtFramePayloadBoolean(true);
        }

        public NtFramePayloadBoolean OnNtFramePayloadDeleteEndpoint(RmContext context, NtFramePayloadDeleteEndpoint query)
        {
            var outboundTunnel = EnforceCryptography(context);

            outboundTunnel.DeleteEndpoint(query.EndpointId);
            return new NtFramePayloadBoolean(true);
        }
    }
}