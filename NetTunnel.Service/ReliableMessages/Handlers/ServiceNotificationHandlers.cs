using NetTunnel.Library.ReliableMessages.Notification;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableMessages.Handlers
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
            var inboundTunnelContext = GetServiceConnectionContext(context);

            inboundTunnelContext.ApplyCryptographyProvider();
        }
    }
}
