using NetTunnel.Library;
using NetTunnel.Service.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine.Endpoints;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Tunnels
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

            tunnel.Core.Logging.Write(NtLogSeverity.Verbose,
                $"End-to-end encryption has been established for '{tunnel.Name}'.");
        }

        /// <summary>
        /// The remote service is asking us to delete a tunnel based on its id
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
        /// 
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

        public void OnNotificationEndpointDisconnect(RmContext context, NotificationEndpointDisconnect notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            tunnel.Core.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint disconnection notification.");

            tunnel.GetEndpointById(notification.EndpointId)?
                .Disconnect(notification.StreamId);
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointExchange notification)
        {
            var tunnel = EnforceCryptographyAndGetTunnel<TunnelInbound>(context);

            var endpoint = tunnel.GetEndpointById(notification.EndpointId)
                .EnsureNotNull($"The outbound tunnel endpoint could not be found: '{notification.EndpointId}'.");

            endpoint.SendEndpointData(notification.StreamId, notification.Bytes);
        }
    }
}
