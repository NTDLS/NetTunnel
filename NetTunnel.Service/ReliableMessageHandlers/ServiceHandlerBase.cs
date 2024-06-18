using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableMessageHandlers
{
    internal class ServiceHandlerBase
    {
        /// <summary>
        /// Enforces that cryptography has been fully initialized and established then returns the Tunnel from the RmContext parameter.
        /// </summary>
        public ServiceConnectionState EnforceLoginCryptographyAndGetServiceConnectionContext(RmContext context)
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
        public ServiceConnectionState EnforceCryptographyAndGetServiceConnectionContext(RmContext context)
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
        public ServiceConnectionState GetServiceConnectionContext(RmContext context)
        {
            if (Singletons.Core.ServiceConnectionStates.TryGetValue(context.ConnectionId, out var connection))
            {
                if (connection.Validate($"{context.TcpClient.Client.RemoteEndPoint}"))
                {
                    return connection;
                }
            }
            throw new Exception("Connection not found.");
        }
    }
}
