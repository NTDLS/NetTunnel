using NetTunnel.Library.Interfaces;
using NetTunnel.Library.Payloads;
using static NetTunnel.Library.Constants;

namespace NetTunnel.Service.TunnelEngine.Endpoints
{
    /// <summary>
    /// Creates the endpoint for a configuration: TCP or UDP, inbound or outbound.
    /// </summary>
    internal static class EndpointFactory
    {
        /// <exception cref="ArgumentException">The configuration contradicts itself (see <see cref="EndpointConfiguration.Validate"/>).</exception>
        public static IEndpoint Create(IServiceEngine serviceEngine, ITunnel tunnel, EndpointConfiguration configuration)
        {
            configuration.Validate();

            return (configuration.Direction, configuration.IsUdp) switch
            {
                (NtDirection.Inbound, false) => new EndpointInbound(serviceEngine, tunnel, configuration),
                (NtDirection.Outbound, false) => new EndpointOutbound(serviceEngine, tunnel, configuration),
                (NtDirection.Inbound, true) => new UdpEndpointInbound(serviceEngine, tunnel, configuration),
                (NtDirection.Outbound, true) => new UdpEndpointOutbound(serviceEngine, tunnel, configuration),
                _ => throw new Exception("Endpoint direction is not well defined.")
            };
        }
    }
}
