﻿using NetTunnel.Library.ReliableMessages.Notification;
using NetTunnel.Service.TunnelEngine;
using NTDLS.NullExtensions;
using NTDLS.ReliableMessaging;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.ReliableMessageHandlers
{
    internal class OutboundTunnelNotificationHandlers : ServiceHandlerBase, IRmMessageHandler
    {
        public void OnNotificationEndpointConnect(RmContext context, NotificationEndpointConnect notification)
        {
            //SEARCH FOR: Process:Endpoint:Connect:004: The remote service has communicated though the tunnel that we need to
            //  establish an associated outbound endpoint connection.
            //var connectionContext = GetServiceConnectionContext(context);

            //var tunnel = context.Endpoint.Parameter.EnsureNotNull();

            Singletons.ServiceEngine.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint connection notification.");

            Singletons.ServiceEngine.Tunnels.EstablishOutboundEndpointConnection(notification.TunnelId, notification.EndpointId, notification.StreamId);
        }

        public void OnNotificationEndpointExchange(RmContext context, NotificationEndpointDataExchange notification)
        {
            //var connectionContext = GetServiceConnectionContext(context);

            var tunnel = (TunnelOutbound)context.Endpoint.Parameter.EnsureNotNull();

            tunnel.SendEndpointData(notification.EndpointId, notification.StreamId, notification.Bytes);

            Singletons.ServiceEngine.Logging.Write(NtLogSeverity.Debug,
                $"Received endpoint data exchange.");
        }
    }
}