using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableMessageHandlers
{
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

            Singletons.Core.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint connection notification.");

            Singletons.Core.Tunnels.EstablishOutboundEndpointConnection(notification.TunnelId, notification.EndpointId, notification.StreamId);
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointExchange notification)
        {
            var connectionContext = GetServiceConnectionContext(context);

            Singletons.Core.Tunnels.SendEndpointData(notification.TunnelId, notification.EndpointId, notification.StreamId, notification.Bytes);

            Singletons.Core.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint data exchange.");
        }
    }
}
