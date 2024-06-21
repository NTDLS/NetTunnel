﻿using NetTunnel.Library.Payloads;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Library.ReliablePayloads.Notification
{
    public class NotificationTunnelDeletion : IRmNotification
    {
        public DirectionalKey TunnelKey { get; set; }

        public NotificationTunnelDeletion(DirectionalKey tunnelKey)
        {
            TunnelKey = tunnelKey;
        }
    }
}