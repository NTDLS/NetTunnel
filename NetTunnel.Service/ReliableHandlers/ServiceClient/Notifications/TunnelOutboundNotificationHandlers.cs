using NetTunnel.Library.ReliablePayloads.Notification.ServiceToService;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableHandlers.ServiceClient.Notifications
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
        public S2SNotificationEndpointConnectQueryReply OnNotify(RmContext context, S2SNotificationEndpointConnectQuery notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.Logger.Verbose($"Received endpoint connection notification.");

                Singletons.ServiceEngine.Tunnels.EstablishOutboundEndpointConnection(
                    notification.TunnelKey.SwapDirection(), notification.EndpointId, notification.EdgeId);

                return new S2SNotificationEndpointConnectQueryReply();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The remote service is letting us know that edge data has been received on an endpoint and is giving
        ///      it to the local service to that it can be delivered to the associated endpoint edge connection.
        /// </summary>
        public void OnNotify(RmContext context, S2SNotificationEndpointDataExchange notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                tunnel.WriteEndpointEdgeData(notification.EndpointId, notification.EdgeId, notification.PacketSequence, notification.Bytes);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The remote service is letting us know that it is deleting the given tunnel.
        /// </summary>
        public void OnNotify(RmContext context, S2SNotificationTunnelDeletion notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Tunnels.DeleteBothEndsOfTunnel(notification.TunnelKey);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The remote service is letting us know that it is deleting the given endpoint.
        /// </summary>
        public void OnNotify(RmContext context, S2SNotificationEndpointDeletion notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Tunnels.DeleteEndpoint(notification.TunnelKey, notification.EndpointId, null);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The remote service is letting us know that an edge connection has been terminated to the given endpoint.
        /// </summary>
        public void OnNotify(RmContext context, S2SNotificationEndpointDisconnect notification)
        {
            try
            {
                var tunnel = EnforceLoginCryptographyAndGetTunnel(context);

                Singletons.ServiceEngine.Tunnels.DisconnectEndpointEdge(notification.TunnelKey, notification.EndpointId, notification.EdgeId);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }
    }
}
