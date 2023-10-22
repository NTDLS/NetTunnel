using static NetTunnel.Library.Constants;

namespace NetTunnel.Library.Types
{
    public interface ITunnel
    {
        /// <summary>
        /// This ID is distinct among an instance of the service but the associated remote service tunnel has the same id.
        /// </summary>
        public Guid PairId { get; }
        public string Name { get; }

        /// <summary>
        /// Stub of function that would be used to send a basic message over the tunnel. You see,
        /// once a tunnel connection is made, we can use it instead of the management port for communication.
        /// This is because this is the ONLY way we can reliably communicate bidirectionally.
        /// </summary>
        /// <param name="message"></param>
        public void DispatchMessage(string message);

        public void AddEndpoint(NtEndpointInboundConfiguration endpointInbound,
            NtEndpointOutboundConfiguration endpointOutbound, EndpointDirection whichIsLocal);
    }
}
