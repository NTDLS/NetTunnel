using NetTunnel.Service.TunnelEngine;
using NTDLS.Helpers;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableHandlers
{
    internal class TunnelOutboundHandlersBase
    {
        /// <summary>
        /// Enforces that the client is logged in and that encryption has been established. Otherwise throws an exception.
        /// </summary>
        public TunnelOutbound EnforceLoginCryptographyAndGetTunnel(RmContext context)
        {
            try
            {
                var tunnel = (TunnelOutbound)context.Endpoint.Parameter.EnsureNotNull();

                tunnel.EnforceLogin();

                return tunnel;
            }
            catch (Exception ex)
            {
                Singletons.ServiceEngine.Logger.Exception(ex);
                throw;
            }
        }
    }
}
