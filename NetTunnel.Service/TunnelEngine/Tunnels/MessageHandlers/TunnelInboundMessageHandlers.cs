using NetTunnel.Library;
using NetTunnel.Service.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels.MessageHandlers
{
    internal class TunnelInboundMessageHandlers : TunnelMessageHandlerBase, IRmMessageHandler
    {
        /// <summary>
        /// The remote service is letting us know that they are about to start using the cryptography provider,
        /// so we need to apply the one that we have ready on this end.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationApplyCryptography(RmContext context, NotificationApplyCryptography notification)
        {
            var tunnel = GetTunnel<TunnelInbound>(context);

            tunnel.ApplyCryptographyProvider();
        }

        /// <summary>
        /// The remote service is asking us to delete an inbound tunnel based on its id.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationDeleteTunnel(RmContext context, NotificationDeleteTunnel notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            tunnel.Core.InboundTunnels.Delete(notification.TunnelId);
            tunnel.Core.InboundTunnels.SaveToDisk();
        }

        /// <summary>
        /// The remote service is letting us know that a that it just received a connection on an inbound endpoint.
        ///  We need to find the associated outbound endpoint and make the associated outbound connection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="notification"></param>
        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

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
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

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
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            var endpoint = tunnel.GetEndpointById(notification.EndpointId)
                .EnsureNotNull($"The outbound tunnel endpoint could not be found: '{notification.EndpointId}'.");

            endpoint.SendEndpointData(notification.StreamId, notification.Bytes);
        }
    }
}
