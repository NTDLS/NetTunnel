using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableMessages.Handlers
{
    internal class ServiceHandlerBase
    {
        /// <summary>
        /// Enforces that cryptography has been fully initialized and established then returns the Tunnel from the RmContext parameter.
        /// </summary>
        public ServiceConnectionContext EnforceLoginCryptographyAndGetServiceConnectionContext(RmContext context)
        {
            var tunnelContext = GetServiceConnectionContext(context);

            if (!tunnelContext.IsAuthenticated || !tunnelContext.SecureKeyExchangeIsComplete)
            {
                throw new Exception("Cryptography has not fully initialized and applied.");
            }
            return tunnelContext;
        }

        /// <summary>
        /// Enforces that cryptography has been fully initialized and established then returns the Tunnel from the RmContext parameter.
        /// </summary>
        public ServiceConnectionContext EnforceCryptographyAndGetServiceConnectionContext(RmContext context)
        {
            var tunnelContext = GetServiceConnectionContext(context);

            if (!tunnelContext.SecureKeyExchangeIsComplete)
            {
                throw new Exception("Cryptography has not fully initialized and applied.");
            }
            return tunnelContext;
        }

        /// <summary>
        /// Returns the Tunnel from the RmContext parameter.
        /// </summary>
        public ServiceConnectionContext GetServiceConnectionContext(RmContext context)
        {
            if (Singletons.Core.InboundTunnelConnections.TryGetValue(context.ConnectionId, out var connection))
            {
                return connection;
            }
            throw new Exception("Connection not found.");
        }
    }
}
