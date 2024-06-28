using NetTunnel.Library.ReliablePayloads.Notification.UI;
using NTDLS.ReliableMessaging;

namespace NetTunnel.UI.ReliableHandlers.Notifications
{
    /// <summary>
    /// Each outbound tunnel makes its own connection using an RmClient. These are the handlers for each outbound tunnel.
    /// </summary>
    internal class old_UINotificationHandlers : IRmMessageHandler
    {
        public void OnNotify(RmContext context, UILoggerNotification notification)
        {
            try
            {
                //tunnel.WriteEndpointEdgeData(notification.EndpointId, notification.EdgeId, notification.Bytes);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
