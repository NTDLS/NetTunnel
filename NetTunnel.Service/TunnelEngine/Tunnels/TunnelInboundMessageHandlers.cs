using NetTunnel.Library;
using NetTunnel.Service.FramePayloads.Notifications;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    public class TunnelInboundMessageHandlers : IRmMessageHandler
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

        public void OnNtFramePayloadEncryptionReady(RmContext context, NtFramePayloadEncryptionReady notification)
        {
            var inboundTunnel = (context.Endpoint.Parameter as TunnelInbound).EnsureNotNull();

            inboundTunnel.ApplyCryptographyProvider();
            inboundTunnel.Core.Logging.Write(NtLogSeverity.Verbose,
                $"End-to-end encryption has been established for '{inboundTunnel.Name}'.");
        }

        public void OnNtFramePayloadMessage(RmContext context, NtFramePayloadMessage notification)
        {
            var inboundTunnel = EnforceCryptography(context);

            inboundTunnel.Core.Logging.Write(NtLogSeverity.Debug,
                $"RPC Message: '{notification.Message}'");
        }

        public void OnNtFramePayloadDeleteTunnel(RmContext context, NtFramePayloadDeleteTunnel notification)
        {
            var inboundTunnel = EnforceCryptography(context);

            inboundTunnel.Core.InboundTunnels.Delete(notification.TunnelId);
            inboundTunnel.Core.InboundTunnels.SaveToDisk();
        }

        public void On(RmContext context, NtFramePayloadEndpointConnect notification)
        {
            var inboundTunnel = EnforceCryptography(context);

            inboundTunnel.Core.Logging.Write(Constants.NtLogSeverity.Debug,
                $"Received endpoint connection notification.");

            inboundTunnel.Endpoints.OfType<EndpointOutbound>().Where(o => o.EndpointId == notification.EndpointId).FirstOrDefault()?
                .EstablishOutboundEndpointConnection(notification.StreamId);
        }

        public void OnNtFramePayloadEndpointDisconnect(RmContext context, NtFramePayloadEndpointDisconnect notification)
        {
            var inboundTunnel = EnforceCryptography(context);

            inboundTunnel.Core.Logging.Write(Constants.NtLogSeverity.Debug,
                $"Received endpoint disconnection notification.");

            inboundTunnel.GetEndpointById(notification.EndpointId)?
                .Disconnect(notification.StreamId);
        }

        public void OnNtFramePayloadEndpointExchange(RmContext context, NtFramePayloadEndpointExchange notification)
        {
            var inboundTunnel = EnforceCryptography(context);

            inboundTunnel.GetEndpointById(notification.EndpointId)?
                .SendEndpointData(notification.StreamId, notification.Bytes);
        }
    }
}
