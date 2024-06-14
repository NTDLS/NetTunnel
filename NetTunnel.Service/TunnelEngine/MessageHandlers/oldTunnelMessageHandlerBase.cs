using NetTunnel.Service.TunnelEngine.Tunnels;
using NTDLS.NullExtensions;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.MessageHandlers
{
    internal class oldTunnelMessageHandlerBase
    {
        /// <summary>
        /// Enforces that cryptography has been fully initialized and established then returns the Tunnel from the RmContext parameter.
        /// </summary>
        public T EnforceCryptographyAndGetTunnel<T>(RmContext context) where T : class, ITunnel
        {
            var inboundTunnel = (context.Endpoint.Parameter as T).EnsureNotNull();
            if (!inboundTunnel.SecureKeyExchangeIsComplete)
            {
                throw new Exception("Cryptography has not fully initialized and applied.");
            }
            return inboundTunnel;
        }


        /// <summary>
        /// Returns the Tunnel from the RmContext parameter.
        /// </summary>
        public T GetTunnel<T>(RmContext context) where T : class, ITunnel
            => (context.Endpoint.Parameter as T).EnsureNotNull();
    }
}
