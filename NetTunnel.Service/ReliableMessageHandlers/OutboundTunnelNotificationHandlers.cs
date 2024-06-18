using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableMessageHandlers
{
    internal class OutboundTunnelNotificationHandlers : ServiceHandlerBase, IRmMessageHandler
    {
        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            //SEARCH FOR: Process:Endpoint:Connect:004: The remote service has communicated though the tunnel that we need to
            //  establish an associated outbound endpoint connection.
            var connectionContext = GetServiceConnectionContext(context);

            Singletons.Core.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint connection notification.");

            Singletons.Core.Tunnels.EstablishOutboundEndpointConnection(notification.TunnelId, notification.EndpointId, notification.StreamId);
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointExchange notification)
        {
            var connectionContext = GetServiceConnectionContext(context);

            Singletons.Core.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint data exchange.");
        }
    }
}
