using NetTunnel.Library;
using NetTunnel.Service.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal class TunnelOutboundMessageHandlers : TunnelMessageHandlerBase, IRmMessageHandler
    {
        public void OnNotificationDeleteTunnel(RmContext context, NotificationDeleteTunnel notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.Core.OutboundTunnels.Delete(notification.TunnelId);
            tunnel.Core.OutboundTunnels.SaveToDisk();
        }

        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.Core.Logging.Write(NtLogSeverity.Debug, $"Received endpoint connection notification.");

            tunnel.Endpoints.OfType<EndpointOutbound>().Where(o => o.EndpointId == notification.EndpointId).FirstOrDefault()?
                .EstablishOutboundEndpointConnection(notification.StreamId);
        }

        public void OnNotificationEndpointDisconnect(RmContext context, NotificationEndpointDisconnect notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.Core.Logging.Write(NtLogSeverity.Debug, $"Received endpoint disconnection notification.");

            tunnel.GetEndpointById(notification.EndpointId)?
                .Disconnect(notification.StreamId);
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointExchange notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.GetEndpointById(notification.EndpointId)
                .EnsureNotNull($"The outbound tunnel endpoint could not be found: '{notification.EndpointId}'.");

            endpoint.SendEndpointData(notification.StreamId, notification.Bytes);
        }
    }
}
