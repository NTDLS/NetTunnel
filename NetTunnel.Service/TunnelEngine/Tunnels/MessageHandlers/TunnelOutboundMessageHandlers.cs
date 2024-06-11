using NetTunnel.Library;
using NetTunnel.Service.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels.MessageHandlers
{
    internal class TunnelOutboundMessageHandlers : TunnelMessageHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// The remote service is asking us to delete an outbound tunnel based on its id.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationDeleteTunnel(RmContext context, NotificationDeleteTunnel notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.Core.OutboundTunnels.Delete(notification.TunnelId);
            tunnel.Core.OutboundTunnels.SaveToDisk();
        }

        /// <summary>
        /// The remote service is letting us know that a that it just received a connection on an inbound endpoint.
        ///  We need to find the associated outbound endpoint and make the associated outbound connection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.Core.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint connection notification.");

            tunnel.Endpoints.OfType<EndpointOutbound>()
                .Where(o => o.EndpointId == notification.EndpointId).FirstOrDefault()?
                .EstablishOutboundEndpointConnection(notification.StreamId);
        }

        /// <summary>
        /// The remote service is letting us know that an endpoint was disconnected from whatever it was connected to.
        /// We need to find the associated endpoint on this end and disconnect it as well.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationEndpointDisconnect(RmContext context, NotificationEndpointDisconnect notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            tunnel.Core.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint disconnection notification.");

            tunnel.GetEndpointById(notification.EndpointId)?
                .Disconnect(notification.StreamId);
        }

        /// <summary>
        /// The remote service is letting us know that is has received data from a connected endpoint.
        /// We need to find the associated endpoint on this end and send it the data which was received.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointExchange notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelOutbound>(context);

            var endpoint = tunnel.GetEndpointById(notification.EndpointId)
                .EnsureNotNull($"The outbound tunnel endpoint could not be found: '{notification.EndpointId}'.");

            endpoint.SendEndpointData(notification.StreamId, notification.Bytes);
        }
    }
}
