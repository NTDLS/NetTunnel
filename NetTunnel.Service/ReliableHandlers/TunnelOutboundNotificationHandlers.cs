using NetTunnel.Library.ReliablePayloads.Notification;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableHandlers
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
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Logger.Verbose($"Received endpoint connection notification.");

                Singletons.ServiceEngine.Tunnels.EstablishOutboundEndpointConnection(
                    notification.TunnelKey.SwapDirection(), notification.EndpointId, notification.EdgeId);
            }
            catch (Exception ex)
            {
                Singletons.ServiceEngine.Logger.Exception(ex);
                throw;
            }
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointDataExchange notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                tunnel.WriteEndpointEdgeData(notification.EndpointId, notification.EdgeId, notification.Bytes);
            }
            catch (Exception ex)
            {
                Singletons.ServiceEngine.Logger.Exception(ex);
                throw;
            }
        }

        public void OnNotificationTunnelDeletion(RmContext context, NotificationTunnelDeletion notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Tunnels.DeleteTunnel(notification.TunnelKey);
            }
            catch (Exception ex)
            {
                Singletons.ServiceEngine.Logger.Exception(ex);
                throw;
            }
        }

        public void OnNotificationEndpointDeletion(RmContext context, NotificationEndpointDeletion notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Tunnels.DeleteEndpoint(notification.TunnelKey, notification.EndpointId);
            }
            catch (Exception ex)
            {
                Singletons.ServiceEngine.Logger.Exception(ex);
                throw;
            }
        }
    }
}
