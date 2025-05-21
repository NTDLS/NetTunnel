using NetTunnel.Library.ReliablePayloads.Notification.ServiceToService;
using NetTunnel.Library.ReliablePayloads.Notification.UI;
using NetTunnel.Library.ReliablePayloads.Notification.UIOrService;
using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableHandlers.Service.Notifications
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefore all inbound tunnels connect to it.
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
        public UOSNotificationApplyCryptographyQueryReply OnNotify(RmContext context, UOSNotificationApplyCryptographyQuery notification)
        {
            try
            {
                var connectionContext = GetServiceConnectionContext(context);

                connectionContext.ApplyCryptographyProvider();
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }

            return new UOSNotificationApplyCryptographyQueryReply();
        }

        public void OnNotify(RmContext context, S2SNotificationEndpointConnect notification)
        {
            try
            {
                var connectionContext = GetServiceConnectionContext(context);

                Singletons.Logger.Verbose($"Received endpoint connection notification.");

                Singletons.ServiceEngine.Tunnels.EstablishOutboundEndpointConnection(
                    notification.TunnelKey.SwapDirection(), notification.EndpointId, notification.EdgeId);
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
                var connectionContext = GetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.WriteEndpointEdgeData(
                    notification.TunnelKey.SwapDirection(), notification.EndpointId, notification.EdgeId, notification.PacketSequence, notification.Bytes);
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
                var connectionContext = GetServiceConnectionContext(context);

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
                var connectionContext = GetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.DeleteEndpoint(notification.TunnelKey, notification.EndpointId, connectionContext.UserName);
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
                var connectionContext = GetServiceConnectionContext(context);

                Singletons.ServiceEngine.Tunnels.DisconnectEndpointEdge(notification.TunnelKey, notification.EndpointId, notification.EdgeId);
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// The UI is requesting that the local service disconnect the given endpoint edge connection.
        /// </summary>
        public void OnNotify(RmContext context, UINotifyTerminateEndpointEdgeConnection notification)
        {
            try
            {
                var connectionContext = GetServiceConnectionContext(context);

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
