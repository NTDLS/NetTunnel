using NetTunnel.Library;
using NTDLS.ReliableMessaging;

namespace NetTunnel.Service.TunnelEngine.Tunnels
{
    internal class TunnelMessageHandlerBase
    {
        /// <summary>
        /// Enforces that cryptography has been fully initialized and established then returns the Tunnel from the RmContext parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public T GetTunnel<T>(RmContext context) where T : class, ITunnel
            => (context.Endpoint.Parameter as T).EnsureNotNull();
    }
}
