using NetTunnel.Service.TunnelEngine;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.ReliableHandlers.Service
{
    /// <summary>
    /// The NetTunnel service shares one single instance of RmServer and therefor all inbound tunnels connect to it.
    /// 
    /// All Client<->Server communication (whether they be UI or other services with inbound tunnels)
    ///     must pass though these handlers.
    /// </summary>
    internal class ServiceHandlerBase
    {
        /// <summary>
        /// Enforces that cryptography has been fully initialized and established then returns the Tunnel from the RmContext parameter.
        /// </summary>
        public ServiceConnectionState EnforceLoginCryptographyAndGetServiceConnectionContext(RmContext context)
        {
            try
            {
                var tunnelContext = GetServiceConnectionContext(context);

                if (!tunnelContext.IsAuthenticated || !tunnelContext.SecureKeyExchangeIsComplete)
                {
                    throw new Exception("Cryptography has not fully initialized and applied.");
                }
                return tunnelContext;
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Enforces that cryptography has been fully initialized and established then returns the Tunnel from the RmContext parameter.
        /// </summary>
        public ServiceConnectionState EnforceCryptographyAndGetServiceConnectionContext(RmContext context)
        {
            try
            {
                var tunnelContext = GetServiceConnectionContext(context);

                if (!tunnelContext.SecureKeyExchangeIsComplete)
                {
                    throw new Exception("Cryptography has not fully initialized and applied.");
                }
                return tunnelContext;
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Returns the Tunnel from the RmContext parameter.
        /// </summary>
        public ServiceConnectionState GetServiceConnectionContext(RmContext context)
        {
            try
            {
                if (Singletons.ServiceEngine.ServiceConnectionStates.TryGetValue(context.ConnectionId, out var connection))
                {
                    if (connection.Validate($"{context.TcpClient.Client.RemoteEndPoint}"))
                    {
                        return connection;
                    }
                }
                throw new Exception("Connection not found.");
            }
            catch (Exception ex)
            {
                Singletons.Logger.Exception(ex);
                throw;
            }
        }
    }
}
