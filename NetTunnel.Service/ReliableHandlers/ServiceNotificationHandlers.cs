using NetTunnel.Library.ReliablePayloads.Notification;
using NetTunnel.Service.TunnelEngine;
using NTDLS.Helpers;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableHandlers
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
            try
            {
                var connectionContext = GetServiceConnectionContext(context);

                connectionContext.ApplyCryptographyProvider();
            }
            catch (Exception ex)
            {
                Singletons.ServiceEngine.Logger.Exception(ex);
                throw;
            }
        }

        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            try
            {
                var connectionContext = GetServiceConnectionContext(context);

                Singletons.ServiceEngine.Logger.Verbose($"Received endpoint connection notification.");

                Singletons.ServiceEngine.Tunnels.EstablishOutboundEndpointConnection(
                    notification.TunnelKey.EnsureNotNull().SwapDirection(), notification.EndpointId, notification.StreamId);
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
                var connectionContext = GetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.SendEndpointData(
                    notification.TunnelKey.EnsureNotNull().SwapDirection(), notification.EndpointId, notification.StreamId, notification.Bytes);

                //Singletons.ServiceEngine.Logger.Debug($"Received endpoint data exchange.");
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
                var connectionContext = GetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.DeleteTunnel(notification.TunnelKey.EnsureNotNull());
            }
            catch (Exception ex)
            {
                Singletons.ServiceEngine.Logger.Exception(ex);
                throw;
            }
        }
    }
}
