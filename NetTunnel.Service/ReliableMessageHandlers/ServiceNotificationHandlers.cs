using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableMessageHandlers
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefor all inbound tunnels connect to it.
    /// 
    /// All Client<->Server notifications communication (whether they be UI or other services with inbound tunnels)
    ///     must pass notification though these handlers.
    /// </summary>
    internal class ServiceNotificationHandlers : ServiceHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// The remote service is letting us know that they are about to start using the cryptography provider,
        /// so we need to apply the one that we have ready on this end.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationApplyCryptography(RmContext context, NotificationApplyCryptography notification)
        {
            var connectionContext = GetServiceConnectionContext(context);

            connectionContext.ApplyCryptographyProvider();
        }

        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            var connectionContext = GetServiceConnectionContext(context);

            Singletons.ServiceEngine.Logger.Verbose($"Received endpoint connection notification.");

            Singletons.ServiceEngine.Tunnels.EstablishOutboundEndpointConnection(notification.TunnelId, notification.EndpointId, notification.StreamId);
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointDataExchange notification)
        {
            var connectionContext = GetServiceConnectionContext(context);

            Singletons.ServiceEngine.Tunnels.SendEndpointData(notification.TunnelId, notification.EndpointId, notification.StreamId, notification.Bytes);

            //Singletons.ServiceEngine.Logger.Debug($"Received endpoint data exchange.");
        }
    }
}
