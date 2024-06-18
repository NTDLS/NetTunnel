using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableMessageHandlers
{
    /// <summary>
    /// Each outbound tunnel makes its own connection using an RmClient. These are the handlers for each outbound tunnel.
    /// </summary>
    internal class TunnelOutboundNotificationHandlers : TunnelOutboundHandlersBase, IRmMessageHandler
    {
        /// <summary>
        ///SEARCH FOR: Process:Endpoint:Connect:004: The remote service has communicated though the tunnel that we need to
        ///  establish an associated outbound endpoint connection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

            Singletons.ServiceEngine.Logger.Verbose($"Received endpoint connection notification.");

            Singletons.ServiceEngine.Tunnels.EstablishOutboundEndpointConnection(notification.TunnelId, notification.EndpointId, notification.StreamId);
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointDataExchange notification)
        {
            var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

            tunnel.SendEndpointData(notification.EndpointId, notification.StreamId, notification.Bytes);

            //Singletons.ServiceEngine.Logger.Debug($"Received endpoint data exchange.");
        }
    }
}
