using NetTunnel.Service.TunnelEngine;
using NTDLS.NullExtensions;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableMessageHandlers
{
    internal class TunnelOutboundHandlersBase
    {
        /// <summary>
        /// Enforces that the client is logged in and that encryption has been established. Otherwise throws an exception.
        /// </summary>
        public TunnelOutbound EnforceLoginCryptographyAndGetTunnel(RmContext context)
        {
            var tunnel = (TunnelOutbound)context.Endpoint.Parameter.EnsureNotNull();

            tunnel.EnforceLogin();

            return tunnel;
        }
    }
}
