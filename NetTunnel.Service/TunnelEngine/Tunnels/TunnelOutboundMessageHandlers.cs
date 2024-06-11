using NetTunnel.Library;
using NetTunnel.Service.FramePayloads.Notifications;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    public class TunnelOutboundMessageHandlers : IRmMessageHandler
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

        public void OnNtFramePayloadMessage(RmContext context, NtFramePayloadMessage notification)
        {
            var outboundTunnel = EnforceCryptography(context);

            outboundTunnel.Core.Logging.Write(NtLogSeverity.Debug, $"RPC Message: '{notification.Message}'");
        }

        public void OnNtFramePayloadDeleteTunnel(RmContext context, NtFramePayloadDeleteTunnel notification)
        {
            var outboundTunnel = EnforceCryptography(context);

            outboundTunnel.Core.OutboundTunnels.Delete(notification.TunnelId);
            outboundTunnel.Core.OutboundTunnels.SaveToDisk();
        }

        public void OnNtFramePayloadEndpointConnect(RmContext context, NtFramePayloadEndpointConnect notification)
        {
            var outboundTunnel = EnforceCryptography(context);

            outboundTunnel.Core.Logging.Write(NtLogSeverity.Debug, $"Received endpoint connection notification.");

            outboundTunnel.Endpoints.OfType<EndpointOutbound>().Where(o => o.EndpointId == notification.EndpointId).FirstOrDefault()?
                .EstablishOutboundEndpointConnection(notification.StreamId);
        }

        public void OnNtFramePayloadEndpointDisconnect(RmContext context, NtFramePayloadEndpointDisconnect notification)
        {
            var outboundTunnel = EnforceCryptography(context);

            outboundTunnel.Core.Logging.Write(NtLogSeverity.Debug, $"Received endpoint disconnection notification.");

            outboundTunnel.GetEndpointById(notification.EndpointId)?
                .Disconnect(notification.StreamId);
        }

        public void OnNtFramePayloadEndpointExchange(RmContext context, NtFramePayloadEndpointExchange notification)
        {
            var outboundTunnel = EnforceCryptography(context);

            //Core.Logging.Write(NtLogSeverity.Debug, $"Exchanging {exchange.Bytes.Length:n0} bytes.");

            outboundTunnel.GetEndpointById(notification.EndpointId)?
                    .SendEndpointData(notification.StreamId, notification.Bytes);
        }
    }
}